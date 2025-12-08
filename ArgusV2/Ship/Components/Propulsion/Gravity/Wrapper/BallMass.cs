using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BallMass : Mass
    {
        private IMySpaceBall _ball;
        private BalancedMassSystem _massSystem;
        private ControllableShip _ship;
        private float _cachedVirtualMass = 20000;
        private bool _wasActive;
        public BallMass(IMySpaceBall ball, BalancedMassSystem massSystem, ControllableShip ship)
        {
            _ball = ball;
            _massSystem = massSystem;
            _ship = ship;
        }

        public bool BalancerAllowed { get; set; } = true;

        public bool GeneratorRequested => _massSystem.Enabled;

        public bool IsActive => BalancerAllowed && GeneratorRequested;
        public override AT_Vector3D Position => _ball.GetPosition();

        public override double AbsoluteVirtualMass
        {
            get
            {
                return _ball.VirtualMass;
            }
            set
            {
                _ball.VirtualMass = (float)value;
            }
        }

        public override float BalancerVirtualMass
        {
            get
            {
                return _cachedVirtualMass;
            }
            set
            {
                _cachedVirtualMass = value;
                if (GeneratorRequested) _ball.VirtualMass = _cachedVirtualMass;
            }
        } 
        public override Vector3I GridPosition => _ball.Position;

        public AT_Vector3D Moment => AbsoluteVirtualMass * ((AT_Vector3D)GridPosition * _ship.GridSize - _ship.LocalCenterOfMass);
        //public AT_Vector3D Moment => AbsoluteVirtualMass * (_ball.GetPosition() - _ship.Controller.CenterOfMass);
   

        public bool UpdateState()
        {
            var r = false;

            if (IsActive != _wasActive)
            {
                _ball.Enabled = IsActive;
                _ball.VirtualMass = _cachedVirtualMass;
            }
            _wasActive = IsActive;
            return r;
        }
    }
}