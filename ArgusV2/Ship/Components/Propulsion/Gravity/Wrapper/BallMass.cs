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
        public BallMass(IMySpaceBall ball, BalancedMassSystem massSystem, ControllableShip ship)
        {
            _ball = ball;
            _massSystem = massSystem;
            _ship = _ship;
        }

        public bool BalancerAllowed { get; set; } = true;

        public bool GeneratorRequested => _massSystem.Enabled;

        public bool IsActive => BalancerAllowed && GeneratorRequested;
        public override AT_Vector3D Position => _ball.GetPosition();
        public override double AbsoluteVirtualMass => _ball.VirtualMass;
        public override double BalancerVirtualMass => BalancerAllowed ? _ball.VirtualMass : 0;
        public override Vector3I GridPosition => _ball.Position;
        public AT_Vector3D Moment => AbsoluteVirtualMass * (_ball.GetPosition() - _ship.Controller.CenterOfMass);

        public bool UpdateState()
        {
            var r = false;
            _ball.Enabled = IsActive;
            return r;
        }
    }
}