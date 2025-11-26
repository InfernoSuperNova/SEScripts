using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BallMass : Mass
    {
        private IMySpaceBall _ball;
        private BalancedMassSystem _massSystem;
        public BallMass(IMySpaceBall ball, BalancedMassSystem massSystem)
        {
            _ball = ball;
            _massSystem = massSystem;
        }

        public bool BalancerAllowed { get; set; } = true;

        public bool GeneratorRequested => _massSystem.Enabled;

        public bool IsActive => BalancerAllowed && GeneratorRequested;
        public override double AbsoluteVirtualMass => _ball.VirtualMass;
        public override double BalancerVirtualMass => BalancerAllowed ? _ball.VirtualMass : 0;

        public void UpdateState()
        {
            _ball.Enabled = IsActive;
        }
    }
}