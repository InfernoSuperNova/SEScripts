using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using IngameScript.Ship;
using IngameScript.Ship.Components;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using IngameScript.Helper;
using IngameScript.TruncationWrappers;

namespace IngameScript
{
    /// <summary>
    /// Represents a friendly ship that can be directly referenced by, but should not be controlled by Argus. Should be able to be promoted to a ControllableShip though.
    /// </summary>
    public class SupportingShip : ArgusShip 
    {
        private readonly List<TargetTracker> _targetTrackers;
        protected readonly IMyCubeGrid _grid;

        
        public SupportingShip(IMyCubeGrid grid, List<IMyTerminalBlock> trackerBlocks)
        {
            Program.LogLine("New SupportingShip : ArgusShip", LogLevel.Debug);
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

            // if (gun == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a gun.");
            // if (rotor == null) throw new Exception($"FATAL: Grid '{grid.CustomName}' tracker group missing a rotor.");


            if (gun != null && rotor != null)
            {
                Program.LogLine("Setting up trackers", LogLevel.Debug);
                foreach (var tracker in _targetTrackers)
                {
                    Program.LogLine($"Set up tracker: {tracker.CustomName}");
                    var block = tracker.Block;
                    block.ClearTools();
                    block.AddTool(gun);
                    block.AzimuthRotor = null; // These aren't intended to actually shoot at anything, just used for target designation and scanning
                    block.ElevationRotor = rotor;
                    block.AIEnabled = true;
                    block.CustomName = Config.Tracker.SearchingName;
                }

                if (_targetTrackers.Count <= 0) Program.LogLine("No target trackers in group", LogLevel.Warning);
            }
            else Program.LogLine($"Gun/rotor not present in group: {Config.String.TrackerGroupName}, cannot setup trackers", LogLevel.Warning);
            
            _grid = grid;
        }
        
        
        public override AT_Vector3D Position => _grid.GetPosition();
        public override AT_Vector3D Velocity => CVelocity;
        public override AT_Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;
        public override float GridSize => _grid.GridSize;

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
                    if (tracker.JustLostTarget && !tracker.Enabled)
                    {
                        tracker.Enabled = true;
                        tracker.CustomName = Config.Tracker.SearchingName;

                        if (tracker.TrackedShip != null)
                        {
                            tracker.TrackedShip.Defunct = true;
                            tracker.TrackedShip.Tracker = null;
                        }
                        
                        tracker.TrackedShip = null;

                        
                    }
                    else if (tracker.JustInvalidated)
                    {
                        tracker.Enabled = true;
                        tracker.CustomName = Config.Tracker.SearchingName;
                    }

                    continue;
                }
                // Tracker currently has a target
                if (tracker.TargetedEntity != 0) continue;
                
                AT_DetectedEntityInfo target = tracker.GetTargetedEntity();
                
                TargetTracker existingTracker;
                // Already tracked by this or another tracker
                if (ShipManager.HasNonDefunctTrackableShip(target.EntityId, out existingTracker))
                {
                    if (existingTracker == tracker && tracker.Enabled) tracker.Enabled = false;
                    existingTracker.TrackedShip.AddTrackedBlock(target, null, tracker);
                    continue;
                }
                // Newly detected ship — register it
                var trackableShip = ShipManager.AddTrackableShip(tracker, target.EntityId, target);
                tracker.TrackedShip = trackableShip;
                tracker.CustomName = Config.Tracker.TrackingName;
                tracker.Enabled = false;
                tracker.TargetedEntity = target.EntityId;   
                
            }
        }
    }
}