using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using IngameScript.Ship;
using IngameScript.Ship.Components;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    /// <summary>
    /// Represents a friendly ship that can be directly referenced by, but should not be controlled by Argus. Should be able to be promoted to a ControllableShip though.
    /// </summary>
    public class SupportingShip : ArgusShip 
    {
        private readonly List<TargetTracker> _targetTrackers;
        private readonly IMyCubeGrid _grid;

        
        public SupportingShip(IMyCubeGrid grid, List<IMyTerminalBlock> trackerBlocks)
        {
            IMyUserControllableGun gun = null;
            IMyMotorStator rotor = null;
            _targetTrackers = new List<TargetTracker>();
            foreach (var block in trackerBlocks)
            {
                var controller = block as IMyTurretControlBlock;
                if (controller != null)
                {
                    
                    _targetTrackers.Add(new TargetTracker(controller));
                    continue;
                }
                gun = gun ?? block as IMyUserControllableGun;
                rotor = rotor ?? block as IMyMotorStator;
            }

            if (gun == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a gun.");
            if (rotor == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a rotor.");



            foreach (var tracker in _targetTrackers)
            {
                var block = tracker.Block;
                block.ClearTools();
                block.AddTool(gun);
                block.AzimuthRotor = null; // These aren't intended to actually shoot at anything, just used for target designation and scanning
                block.ElevationRotor = rotor;
                block.AIEnabled = true;
                block.CustomName = Config.SearchingName;
            }
            
            _grid = grid;
        }
        
        
        public override Vector3D Position => _grid.GetPosition();
        public override Vector3D Velocity => CVelocity;
        public override Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;

        public override string Name => _grid.CustomName;
        public override string ToString() => Name;

        public override void EarlyUpdate(int frame)
        {
            CPreviousVelocity = CVelocity;
            CVelocity = _grid.LinearVelocity;
        }

        public override void LateUpdate(int frame)
        {
            for (int i = _targetTrackers.Count - 1; i >= 0; i--)
            {
                var tracker = _targetTrackers[i];

                // Cleanup destroyed trackers
                if (tracker.Closed)
                {
                    _targetTrackers.RemoveAt(i);
                    continue;
                }

                // Update target state
                tracker.UpdateState();

                // Tracker has no target
                if (!tracker.HasTarget || tracker.Invalid)
                {
                    if (tracker.JustLostTarget)
                    {
                        tracker.Enabled = true;
                        tracker.CustomName = Config.SearchingName;
                        tracker.TrackedShip = null;
                    }
                    else if (tracker.JustInvalidated)
                    {
                        tracker.Enabled = true;
                        tracker.CustomName = Config.SearchingName;
                    }

                    continue;
                }

                // Tracker currently has a target
                if (tracker.TargetedEntity != 0) continue;
                
                var target = tracker.GetTargetedEntity();


                TargetTracker existingTracker;
                // Already tracked by this or another tracker
                if (ShipManager.EntityIdToTracker.TryGetValue(target.EntityId, out existingTracker))
                {
                    if (existingTracker == tracker && tracker.Enabled)
                        tracker.Enabled = false;
                    
                    existingTracker.TrackedShip.AddTrackedBlock(target, null, tracker);
                    continue;
                }

                // Newly detected ship — register it
                var trackableShip = ShipManager.AddTrackableShip(tracker, target.EntityId, target);
                tracker.TrackedShip = trackableShip;
                tracker.CustomName = Config.TrackingName;
                tracker.Enabled = false;
                tracker.TargetedEntity = target.EntityId;
                
            }
        }
    }
}