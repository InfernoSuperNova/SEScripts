using System.Collections.Generic;
using IngameScript.Ship.Components;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship
{
    public class ControllableShip : SupportingShip
    {
        private GyroManager _gyroManager;
        public GunManager Guns;
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
        // TODO: Handler for gravity drive and thrusters (probably just copy and paste from arguslite)

        private TrackableShip _currentTarget;
        private bool _hasTarget;
        
        
        public IMyShipController Controller { get; set; }
        //public IMyFlightMovementBlock AiFlight { get; set; }
        public Vector3D Forward => Controller.WorldMatrix.Forward;
        public Vector3D Up => Controller.WorldMatrix.Up;
        
        public ControllableShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks, List<IMyTerminalBlock> trackerBlocks) : base(grid, trackerBlocks)
        {
            _gyroManager = new GyroManager(blocks);
            Guns = new GunManager(blocks);
            foreach (var block in blocks) // TODO: Proper controller candidate system (and perhaps manager)
            {
                var controller = block as IMyShipController;
                // var aiFlight = block as IMyFlightMovementBlock;
                // var aiOffensive = block as IMyOffensiveCombatBlock;
                if (controller != null) Controller = controller;
            }
        }

        

        public override void EarlyUpdate(int frame)
        {
            base.EarlyUpdate(frame);
            Guns.EarlyUpdate(frame);

        }

        public override void LateUpdate(int frame)
        {
            base.LateUpdate(frame);
            if (!_hasTarget) return;
            var leadPos = GetTargetLeadPosition(_currentTarget, 2000);
            var solution = (leadPos - Position).Normalized();
            var onTarget = solution.Dot(Forward);
            _gyroManager.Rotate(solution, onTarget, Controller);
            Guns.LateUpdate(frame);
        }

        public void UnTarget()
        {
            _currentTarget = null;
            _hasTarget = false;
            _gyroManager.ResetGyroOverrides();
        }

        public void Target()
        {
            _currentTarget = ShipManager.GetForwardTarget(this, Config.LockRange, Config.LockAngle);
            _hasTarget = true;
            if (_currentTarget == null)
            {
                _hasTarget = false;
                _gyroManager.ResetGyroOverrides();
            }
        }
    }
}