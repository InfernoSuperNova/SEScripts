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
using IngameScript.Ship;
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
    /// <summary>
    /// Represents a friendly ship that can be directly referenced by, but should not be controlled by Argus. Should be able to be promoted to a ControllableShip though.
    /// </summary>
    public class SupportingShip : ArgusShip 
    {
        private readonly List<IMyTurretControlBlock> _targetTrackers;
        private readonly IMyCubeGrid _thisGrid;

        
        public SupportingShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks)
        {
            _targetTrackers = new List<IMyTurretControlBlock>();
            foreach (var block in blocks)
            {
                var controller = block as IMyTurretControlBlock;
                if (controller != null)
                {
                    controller.CustomName = "NOT TRACKING";
                    _targetTrackers.Add(controller);
                }
            }
            
            _thisGrid = grid;
        }
        
        
        public override Vector3D Position => _thisGrid.GetPosition();
        public override Vector3D Velocity => CVelocity;
        public override Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;

        public override string Name => _thisGrid.CustomName;
        public override string ToString() => Name;

        public override void PollSensors(int frame)
        {
            CPreviousVelocity = CVelocity;
            CVelocity = _thisGrid.LinearVelocity;
        }

        public override void LateUpdate(int frame)
        {
            for (var index = _targetTrackers.Count - 1; index >= 0; index--)
            {
                var tracker = _targetTrackers[index];

                if (tracker.Closed)
                {
                    _targetTrackers.RemoveAt(index);
                    continue;
                }

                if (!tracker.HasTarget)
                {
                    if (!tracker.Enabled)
                    {
                        tracker.Enabled = true;
                        tracker.CustomName = "NOT TRACKING";
                    }
                    continue;
                }
                
                if (ShipManager.TrackerToEntityId.ContainsKey(tracker)) continue;
                var target = tracker.GetTargetedEntity();   
                if (ShipManager.EntityIdToTrackableShip.ContainsKey(target.EntityId)) continue; // I don't think this check is necessary but might as well keep it anyway

                ShipManager.AddTrackableShip(tracker, target.EntityId);
                tracker.CustomName = "TRACKING";
                tracker.Enabled = false;
            }
        }
    }
}