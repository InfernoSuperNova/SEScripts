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
                if (controller != null) _targetTrackers.Add(controller);
            }
            
            _thisGrid = grid;
            
            // TODO: - get blocks on this grid
        }
        
        
        public override Vector3D Position => _thisGrid.GetPosition();
        public override Vector3D Velocity => CVelocity;
        public override Vector3D Acceleration => CVelocity - CPreviousVelocity;
        

        public override void EarlyUpdate(int frame)
        {
            CPreviousVelocity = CVelocity;
            CVelocity = _thisGrid.LinearVelocity;
        }

        public override void LateUpdate(int frame)
        {
            
        }
    }
}