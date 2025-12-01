using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BlockMass : Mass
    {
        private IMyArtificialMassBlock _mass;
        private BalancedMassSystem _massSystem;
        private ControllableShip _ship;

        private bool _previousActive;
        public BlockMass(IMyArtificialMassBlock mass, BalancedMassSystem massSystem, ControllableShip ship)
        {
            _mass = mass;
            _massSystem = massSystem;
            _ship = ship;
            _mass.Enabled = false;
        }
        
        public bool BalancerAllowed { get; set; } = true;

        public bool GeneratorRequested => _massSystem.Enabled;

        public bool IsActive => BalancerAllowed && GeneratorRequested;
        public override double AbsoluteVirtualMass => _mass.VirtualMass;
        public override double BalancerVirtualMass => BalancerAllowed ? _mass.VirtualMass : 0;
        
        public Vector3D Moment => AbsoluteVirtualMass * (_mass.GetPosition() - _ship.Controller.CenterOfMass);

        public bool UpdateState()
        {
            var stateChanged = false;

            if (_previousActive != IsActive)
            {
                _mass.Enabled = IsActive;
                stateChanged = true;
            }
            _previousActive = IsActive;
            return stateChanged;
        }
    }
}