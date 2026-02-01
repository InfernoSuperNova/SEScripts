using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    /// <summary>
    /// BallMass class.
    /// </summary>
    /// <summary>
    /// BallMass class.
    /// </summary>
    public class BallMass : Mass
    {
        private IMySpaceBall _ball;
        private BalancedMassSystem _massSystem;
        private ControllableShip _ship;
        private float _cachedVirtualMass = 20000;
        private bool _wasActive;
        /// <summary>
        /// BallMass method.
        /// </summary>
        /// <param name="ball">The ball parameter.</param>
        /// <param name="massSystem">The massSystem parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// BallMass method.
        /// </summary>
        /// <param name="ball">The ball parameter.</param>
        /// <param name="massSystem">The massSystem parameter.</param>
        /// <param name="ship">The ship parameter.</param>
        /// <returns>The result of the operation.</returns>
        public BallMass(IMySpaceBall ball, BalancedMassSystem massSystem, ControllableShip ship)
        {
            _ball = ball;
            _massSystem = massSystem;
            _ship = ship;
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