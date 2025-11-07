using System.Collections.Generic;
using IngameScript.Ship.Components;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship
{
    public class ControllableShip : SupportingShip
    {
        private GyroManager _gyroManager;
        private List<IMyUserControllableGun> _guns; // TODO: Abstract into a guns handler
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
        // TODO: Handler for gravity drive and thrusters (probably just copy and paste from arguslite)

        private TrackableShip _currentTarget;
        private bool _hasTarget;
        
        
        public IMyShipController Controller { get; set; }
        public Vector3D Forward => Controller.WorldMatrix.Forward;
        public Vector3D Up => Controller.WorldMatrix.Up;
        
        public ControllableShip(IMyCubeGrid _grid, List<IMyTerminalBlock> blocks) : base(_grid, blocks)
        {
            _gyroManager = new GyroManager(blocks);
            foreach (var block in blocks) // TODO: Proper controller candidate system (and perhaps manager)
            {
                var controller = block as IMyShipController;
                if (controller != null) Controller = controller;
            }
        }

        public override void PollSensors(int frame)
        {
            base.PollSensors(frame);
        }

        public override void LateUpdate(int frame)
        {
            base.LateUpdate(frame);

            if (!_hasTarget) return;
            var leadPos = GetTargetLeadPosition(_currentTarget, 2000);
            var solution = (leadPos - Position).Normalized();
            var onTarget = solution.Dot(Forward);
            _gyroManager.Rotate(solution, onTarget, Controller);
            
        }

        public void UnTarget()
        {
            _currentTarget = null;
            _hasTarget = false;
            _gyroManager.ResetGyroOverrides();
        }

        public void Target()
        {
            _currentTarget = ShipManager.GetForwardTarget(this, 2000, 20);
            _hasTarget = true;
            if (_currentTarget == null)
            {
                _hasTarget = false;
                _gyroManager.ResetGyroOverrides();
            }
        }
    }
}