using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    
    
    // Prefixes are as follows - [sender][type][receiver], so MuD would be Mothership > Unicast > Drone
    public static class SendKeys
    {
        public const string MothershipPosition = "MuD_Position";
        public const string MothershipVelocity = "MuD_Velocity";
        public const string MothershipDirection = "MuD_MDirection";
        public static string DroneDirection = "MuD_Direction";
        public static string DroneDistance = "MuD_Distance";
        public static string DroneTakeOwnership = "MuD_Owned";
    }

    public static class ReceiveKeys
    {
        public const string DroneHeartbeat = "DuM_Heartbeat";
        public const string DroneRegister = "DuM_Register";
        public const string DroneDeregister = "DuM_Deregister";
    }
    partial class Program : MyGridProgram
    {
        // Configs
        private static int _maxHeartbeatMisses = 3600; // 60 seconds, this is temp and we should do it less frequently than update1 eventually
        private static float _escortDistance = 500;  // Distance for drones to maintain
        private static float _minAntennaRange = 5000; // Script will adjust antenna range to keep in contact with drones, this is the lowest it will go
        private static string _groupName = "SDXMothership"; // Group all blocks on your ship under this name

        private IMyShipController _shipController;
        private IMyRadioAntenna _mainAntenna;
        // Listeners
        private static IMyBroadcastListener _droneRegisterListener;
        private static IMyBroadcastListener _droneDeRegisterListener;
        
        
        
        private static List<DroneIgcClient> _drones = new List<DroneIgcClient>();
        private static Dictionary<long, DroneIgcClient> _droneLookup = new Dictionary<long, DroneIgcClient>();
        public Program()
        {
            _droneRegisterListener = IGC.RegisterBroadcastListener(ReceiveKeys.DroneRegister);
            _droneDeRegisterListener = IGC.RegisterBroadcastListener(ReceiveKeys.DroneDeregister);
            _shipController = GridTerminalSystem.GetBlockWithName("MainCockpit") as IMyShipController;
            if (_shipController == null)
            {
                Echo("Ship controller not found! Name a cockpit 'MainCockit'!");
                return;
            }

            var group = GridTerminalSystem.GetBlockGroupWithName(_groupName);
            if (group == null)
            {
                Echo($"Group not found! Create a group called {_groupName} containing all the ship's blocks!");
                return;
            }
            
            List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
            group.GetBlocksOfType(antennas);
            if (antennas == null || antennas[0] == null)
            {
                Echo("No antennas found! Please add an antenna to the group and recompile");
                return;
            }

            _mainAntenna = antennas[0];
            
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update1) != 0)
            {
                Update();
            }
        }

        private void Update()
        {
            // Check for the heartbeats and remove any that haven't responded
            bool sphereNeedsRecalc = CheckDroneHeartbeats();
            
            // Check for deregistered drones
            sphereNeedsRecalc |= DeregisterDrones();
            
            // Check for new drones
            sphereNeedsRecalc |= RegisterDrones();
            
            
            if (sphereNeedsRecalc) SendDroneDirections();
            // Give our drones our new position and velocity
            if (_drones.Count == 0)
            {
                return;
            }

            Vector3D pos = Me.CubeGrid.WorldVolume.Center;
            Vector3 velocity = Me.CubeGrid.LinearVelocity;
            Vector3 forward = _shipController.WorldMatrix.Forward;
            UpdateAntennaRanges(pos);

            for (int i = 0; i < _drones.Count(); i++)
            {
                var drone = _drones[i];
                drone.MothershipPosition = pos;
                drone.MothershipDirection = forward;
                drone.MothershipVelocity = velocity;
            }
            
            Echo($"RunTime: {Runtime.LastRunTimeMs} ms");
        }



        #region Heartbeat
        /// <summary>
        /// Checks for drone hearbeats and removes them from the list if they haven't reported their existence in a while.
        /// </summary>
        /// <returns> True if the drone sphere needs updating. </returns>
        private bool CheckDroneHeartbeats()
        {
            if (_drones.Count == 0) return false; // No drones
            bool sphereNeedsRecalc = UpdateDroneHeartbeats(); // Sets for each drone if the last heartbeat was successful
            return sphereNeedsRecalc || EvaluateDroneHeartbeats();
        }
        /// <summary>
        /// Updates drones heartbeats with true or false. Clears the _droneHeartbeatListener message queue.
        /// </summary>
        private bool UpdateDroneHeartbeats()
        {
            bool sphereNeedsRecalc = false;
            for (int i = 0; i < _drones.Count; i++)
            {
                _drones[i].JustMissedHeartbeat = true;
            }
            
            
            while (IGC.UnicastListener.HasPendingMessage)
            {
                MyIGCMessage message = IGC.UnicastListener.AcceptMessage();

                switch (message.Tag)
                {
                    case ReceiveKeys.DroneHeartbeat:
                        var pos = message.Data as Vector3D? ?? new Vector3D();
                
                        DroneIgcClient droneIgcClient;
                        if (_droneLookup.TryGetValue(message.Source, out droneIgcClient))
                        {
                            Echo("Got heartbeat.");
                            droneIgcClient.JustMissedHeartbeat = false;
                            if (pos != Vector3D.Zero) droneIgcClient.LastSeenPos = pos;
                        }
                        break;
                    case ReceiveKeys.DroneRegister:
                        sphereNeedsRecalc = RegisterDrone(message, sphereNeedsRecalc);
                        break;
                }

                
            }

            return sphereNeedsRecalc;
        }
        /// <summary>
        /// Checks drone heartbeats, and increments the count if they have missed or resets if they don't.
        /// If above the miss limit, removes from both the list and lookup dict.
        /// </summary>
        /// <returns>True if the fibonacci sphere needs updating.</returns>
        private bool EvaluateDroneHeartbeats()
        {
            bool droneRemoved = false;
            for (int i = _drones.Count - 1; i >= 0; i--)
            {
                var drone = _drones[i];
                if (drone.JustMissedHeartbeat)
                {
                    Echo("Drone missed hearbeat " + drone.TimesMissedHeartbeat + "times");
                    if (++drone.TimesMissedHeartbeat >= _maxHeartbeatMisses)
                    {
                        droneRemoved = true;
                        // Drone is no longer connected, remove it
                        _drones.RemoveAt(i);
                        _droneLookup.Remove(drone.Id);
                        Echo("Drone failed to heartbeat in time...");
                    }
                }
                else
                {
                    drone.TimesMissedHeartbeat = 0;
                }
            }

            return droneRemoved;
        }
        #endregion
        
        /// <summary>
        /// Handles deregistering of drones.
        /// </summary>
        /// <returns>True if one or more drones deregistered.</returns>
        private bool DeregisterDrones() // Tbh not sure when this would ever be called.
        {
            bool droneRemoved = _droneDeRegisterListener.HasPendingMessage;
            while (_droneDeRegisterListener.HasPendingMessage)
            {
                var message = _droneDeRegisterListener.AcceptMessage();
                var id = message.Source;
                var drone = _droneLookup[id];
                _droneLookup.Remove(id);
                _drones.Remove(drone);
            }

            return droneRemoved;
        }
        
        /// <summary>
        /// Handles the registering of new drones.
        /// </summary>
        /// <returns>True if one or more drones registered.</returns>
        private bool RegisterDrones()
        {
            bool droneRegistered = false;
            
            while (_droneRegisterListener.HasPendingMessage)
            {
                var message = _droneRegisterListener.AcceptMessage();
                droneRegistered = RegisterDrone(message, droneRegistered);
            }

            return droneRegistered;
        }

        private bool RegisterDrone(MyIGCMessage message, bool droneRegistered)
        {
            var id = message.Source;

            DroneIgcClient droneIgcClient;
            if (_droneLookup.TryGetValue(id, out droneIgcClient)) // This drone is already registered? Probably occurs on recompile uncertainty.
            {
                //Echo("Resetting already registered drone");
                droneIgcClient.Distance = droneIgcClient.Distance;
                droneIgcClient.Direction = droneIgcClient.Direction; // This Resends the value to the drone
                droneIgcClient.TakeOwnership();
                return droneRegistered; 
            }

            Echo("Registering new drone");
            droneIgcClient = new DroneIgcClient(id, IGC);
            droneIgcClient.Distance = _escortDistance;
            _drones.Add(droneIgcClient);
                
            _droneLookup[id] = droneIgcClient;
                
            droneIgcClient.TakeOwnership();
            return true;
        }

        /// <summary>
        /// Generates new fibonacci sphere values for the current assortment of drones and sends them off.
        /// </summary>
        private void SendDroneDirections()
        {
            var directions = FibonacciSphereGenerator.GenerateDirections(_drones.Count); 
            // This should be deterministic and should lead to minimal drone reshuffling when a new drone is added
            for (int i = 0; i < _drones.Count; i++)
            {
                var drone = _drones[i];

                drone.Direction = directions[i];

            }
        }
        
        private void UpdateAntennaRanges(Vector3D pos)
        {
            if (_drones == null) return;
            double maxRange = _minAntennaRange * _minAntennaRange;
            for (int i = 0; i < _drones.Count; i++)
            {
                var drone = _drones[i];
                maxRange = Math.Max(maxRange, (drone.LastSeenPos - pos).LengthSquared());
            }

            var rangeNonSqr = Math.Sqrt(maxRange);
            _mainAntenna.Radius = (float)rangeNonSqr;
        }
    }
}