using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components;
using IngameScript.Ship.Components.Propulsion;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using Base6Directions = VRageMath.Base6Directions;

namespace IngameScript.Ship
{
    public class ControllableShip : SupportingShip
    {
        
        #region Fields
        
        public readonly GunManager Guns;
        private readonly GyroManager _gyroManager;
        private readonly FireController _fireController;
        private readonly PropulsionController _propulsionController;
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler, assign
        
        
        private Vector3D _cachedGravity;
        private bool _gravityValid = false;
        private MyShipMass _cachedMass;
        private bool _massValid = false;
        #endregion
        
        public ControllableShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks, List<IMyTerminalBlock> trackerBlocks) : base(grid, trackerBlocks)
        {
            Program.LogLine("New ControllableShip : SupportingShip : ArgusShip", LogLevel.Debug);
            foreach (var block in blocks) // TODO: Proper controller candidate system (and perhaps manager)
            {
                var controller = block as IMyShipController;
                if (controller != null) Controller = controller;
            }

            // Without a controller, the ship is virtually useless
            if (Controller == null)
            {
                Program.LogLine($"WARNING: Controller not present in group: {Config.GroupName}");
                return;
            }
            _gyroManager = new GyroManager(blocks);
            Guns = new GunManager(blocks, this);
            _fireController = new FireController(this, Guns);
            var _ = Mass; // Manually triggers a (re)calculation of the mass property
            _propulsionController = new PropulsionController(blocks, this);
            
        }
        
        #region Properties
        /// <summary>
        /// Gets the current controller of the grid.
        /// This is subject to change on a per-frame basis.
        /// </summary>
        public IMyShipController Controller { get; private set; }
        
        /// <summary>
        /// Gets the world matrix of the grid.
        /// Should only be used for transforms of directions and positions in/out of grid local space.
        /// </summary>
        public MatrixD WorldMatrix => _grid.WorldMatrix;
        /// <summary>
        /// Gets the controller centric forward of the grid.
        /// This is more useful and player centric as the controller can be arbitrarily orientated in any of 24 possible options.
        /// </summary>
        public Vector3D WorldForward => Controller.WorldMatrix.Forward; 
        /// <summary>
        /// Gets the controller centric up of the grid.
        /// This is more useful and player centric as the controller can be arbitrarily orientated in any of 24 possible options.
        /// </summary>
        public Vector3D WorldUp => Controller.WorldMatrix.Up;
        /// <summary>
        /// Gets the forward of the controller relative to its own internal orientation in the grid.
        /// </summary>
        public Vector3 LocalForward => Base6Directions.Directions[(int)Controller.Orientation.Forward];
        /// <summary>
        /// Gets the directional enum of the forward of the controller relative to its own internal orientation in the grid.
        /// </summary>
        public Direction LocalDirectionForward => (Direction)Controller.Orientation.Forward;
        /// <summary>
        /// Gets the currently targeted grid, if applicable.
        /// </summary>
        public TrackableShip CurrentTarget { get; private set; }

        /// <summary>
        /// Gets if the controller currently has a target.
        /// </summary>
        public bool HasTarget => CurrentTarget != null;

        /// <summary>
        /// Gets the current natural gravity at the position of the controller.
        /// </summary>
        public Vector3D Gravity 
        {
            get
            {
                // We wrap in a lazy cached system as getting the natural gravity of a given worldpoint can be and is pretty slow.
                if (!_gravityValid)
                {
                    _cachedGravity = Controller.GetNaturalGravity();
                    _gravityValid = true; 
                }

                return _cachedGravity;
            }
        }

        /// <summary>
        /// Gets the mass details of the grid. Subject to inaccuracy if being changed rapidly.
        /// </summary>
        public MyShipMass Mass
        {
            get
            {
                // Perhaps there is an opportunity to abstract and reuse this pattern here. Not sure how feasible that would be.
                // Maybe if I have a third one of these.
                if (!_massValid)
                {
                    _cachedMass = Controller.CalculateShipMass();
                    _massValid = true;
                }

                return _cachedMass;
            }
        }
        #endregion
        
        // Could possibly subscribe these and make them private?
        #region Updates
        // Early Update is called in a deterministic, but undefined order. Designed for gathering data.
        // All Early Updates are guaranteed to be called before any Late Updates.
        public override void EarlyUpdate(int frame)
        {
            base.EarlyUpdate(frame);
            Guns.EarlyUpdate(frame);
            _propulsionController.EarlyUpdate(frame);
            
            if ((frame + RandomUpdateJitter) % Polling.GetFramesBetweenPolls(PollFrequency) == 0)
            {
                _gravityValid = false;
                _massValid = false;
            }
        }
        // Late Update is called in a deterministic, but undefined order. Designed for acting on data.
        // All Late Updates are guaranteed to be called before any Early Updates.
        public override void LateUpdate(int frame)
        {
            base.LateUpdate(frame);
            if (HasTarget)
            {
                
                var solution = _fireController.ArbitrateFiringSolution();
                if (solution.TargetPosition == Vector3D.Zero) _gyroManager.ResetGyroOverrides(); // TODO: Don't spam this
                else _gyroManager.Rotate(ref solution);
                
                // TODO: A little inconsistent to put this in a check when the EarlyUpdate is not.
                // Instead guns itself should handle the gatekeeping.
                // Technically this all works the way it should but in a questionable way.
                Guns.LateUpdate(frame); 
                
            }
            _propulsionController.LateUpdate(frame);
        }
        #endregion
        
        #region Targeting
        
        /// <summary>
        /// Gets the ship to search for a new target off it's bow, and set it as the current target if valid. 
        /// </summary>
        public void Target()
        {
            
            CurrentTarget = ShipManager.GetForwardTarget(this, Config.LockRange, Config.LockAngle);
            if (CurrentTarget == null)
            {
                _gyroManager.ResetGyroOverrides();
                Program.LogLine("Couldn't find new target", LogLevel.Warning);
            }
            else
            {
                Program.LogLine("Got new target", LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Unsets the current target on a ship and allows it to resume normal control.
        /// </summary>
        public void UnTarget()
        {
            CurrentTarget = null;
            _gyroManager.ResetGyroOverrides();
        }

        /// <summary>
        /// Gets the current position of the target.
        /// </summary>
        /// <returns>The targets position, or 0 if invalid. </returns>
        public Vector3D GetTargetPosition()
        {
            return CurrentTarget?.Position ?? Vector3D.Zero;
        }
        #endregion
    }
}