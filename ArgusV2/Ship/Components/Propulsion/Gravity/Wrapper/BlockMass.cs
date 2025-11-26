using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BlockMass : Mass
    {
        private IMyArtificialMassBlock _mass;
        private BalancedMassSystem _massSystem;

        private bool _previousActive;
        public BlockMass(IMyArtificialMassBlock mass, BalancedMassSystem massSystem)
        {
            _mass = mass;
            _massSystem = massSystem;
        }
        
        public bool BalancerAllowed { get; set; } = true;

        public bool GeneratorRequested => _massSystem.Enabled;

        public bool IsActive => BalancerAllowed && GeneratorRequested;
        public override double AbsoluteVirtualMass => _mass.VirtualMass;
        public override double BalancerVirtualMass => BalancerAllowed ? _mass.VirtualMass : 0;

        public void UpdateState()
        {
            

            if (_previousActive != IsActive)
                _mass.Enabled = IsActive;
            _previousActive = IsActive;
        }
    }
}