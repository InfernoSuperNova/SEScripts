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
        private GyroManager _gyroManager;
        public GunManager Guns;
        private FireController _fireController;
        private List<IMyLargeTurretBase> _turrets; // TODO: Abstract into a turrets handler
        private PropulsionController _propulsionController;

        private TrackableShip _currentTarget;
        private bool _hasTarget;

        private Vector3D _cachedGravity;
        private bool _gravityValid = false;
        

        public ControllableShip(IMyCubeGrid grid, List<IMyTerminalBlock> blocks, List<IMyTerminalBlock> trackerBlocks) : base(grid, trackerBlocks)
        {
            Program.LogLine("New ControllableShip : SupportingShip : ArgusShip", LogLevel.Debug);
            _gyroManager = new GyroManager(blocks);
            Guns = new GunManager(blocks, this);
            _fireController = new FireController(this, Guns);
            foreach (var block in blocks) // TODO: Proper controller candidate system (and perhaps manager)
            {
                var controller = block as IMyShipController;
                // var aiFlight = block as IMyFlightMovementBlock;
                // var aiOffensive = block as IMyOffensiveCombatBlock;
                if (controller != null) Controller = controller;
            }
            if (Controller == null) Program.LogLine($"WARNING: Controller not present in group: {Config.GroupName}");
            _propulsionController = new PropulsionController(blocks, this);
        }

        public IMyShipController Controller { get; set; }
        //public IMyFlightMovementBlock AiFlight { get; set; }
        public Vector3D Forward => Controller.WorldMatrix.Forward;

        public Vector3 LocalForward => Base6Directions.Directions[(int)Controller.Orientation.Forward];
        public Direction LocalOrientationForward => (Direction)Controller.Orientation.Forward;
        public Vector3D Up => Controller.WorldMatrix.Up;
        public MatrixD WorldMatrix => _grid.WorldMatrix;
        public TrackableShip CurrentTarget => _currentTarget;
        
        public Vector3D Gravity 
        {
            get
            {
                if (!_gravityValid)
                {
                    _cachedGravity = Controller.GetNaturalGravity();
                    _gravityValid = true; 
                }

                return _cachedGravity;
            }
        }

        public override void EarlyUpdate(int frame)
        {
            base.EarlyUpdate(frame);
            Guns.EarlyUpdate(frame);
            _propulsionController.EarlyUpdate(frame);
        }

        public override void LateUpdate(int frame)
        {
            base.LateUpdate(frame);
            if (_hasTarget)
            {
                _gravityValid = false;
            
            
                var solution = _fireController.ArbitrateFiringSolution();
                if (solution.TargetPosition == Vector3D.Zero) _gyroManager.ResetGyroOverrides(); // TODO: Don't spam this
                else _gyroManager.Rotate(ref solution);
                Guns.LateUpdate(frame);
            }
            
            _propulsionController.LateUpdate(frame);
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
                Program.LogLine("Couldn't find new target", LogLevel.Warning);
            }
            else
            {
                Program.LogLine("Got new target", LogLevel.Info);
            }
        }

        public Vector3D GetTargetPosition()
        {
            return _currentTarget?.Position ?? Vector3D.Zero;
        }
    }
}