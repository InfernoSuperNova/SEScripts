using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    /// <summary>
    /// BlockMass class.
    /// </summary>
    /// <summary>
    /// BlockMass class.
    /// </summary>
    public class BlockMass : Mass
    {
        private IMyArtificialMassBlock _mass;
        private BalancedMassSystem _massSystem;
        private ControllableShip _ship;

        private bool _previousActive;
        /// <summary>
        /// BlockMass method.
        /// </summary>
        /// <param name="mass">The mass parameter.</param>
        /// <param name="massSystem">The massSystem parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// BlockMass method.
        /// </summary>
        /// <param name="mass">The mass parameter.</param>
        /// <param name="massSystem">The massSystem parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public override AT_Vector3D Position => _mass.GetPosition();
        public override double AbsoluteVirtualMass
        {
            get { return _mass.VirtualMass; }
            set {  } // Just a stub since can't set virtual mass on a mass block
        }

        public override float BalancerVirtualMass
        {
            get { return BalancerAllowed ? _mass.VirtualMass : 0; }
            set {  }
        }

        public override Vector3I GridPosition => _mass.Position;
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <param name="_mass.GetPosition(">The _mass.GetPosition( parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetPosition method.
        /// </summary>
        /// <param name="_mass.GetPosition(">The _mass.GetPosition( parameter.</param>
        /// <returns>The result of the operation.</returns>
        public AT_Vector3D Moment => AbsoluteVirtualMass * (_mass.GetPosition() - _ship.Controller.CenterOfMass);

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