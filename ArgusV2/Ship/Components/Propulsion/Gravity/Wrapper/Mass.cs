using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public abstract class Mass
    {
        /// <summary>
        /// Gets or sets the Position.
        /// </summary>
        /// <summary>
        /// Gets or sets the Position.
        /// </summary>
        public abstract AT_Vector3D Position { get; }
        /// <summary>
        /// Gets or sets the AbsoluteVirtualMass.
        /// </summary>
        /// <summary>
        /// Gets or sets the AbsoluteVirtualMass.
        /// </summary>
        public abstract double AbsoluteVirtualMass { get; set; }
        /// <summary>
        /// Gets or sets the BalancerVirtualMass.
        /// </summary>
        /// <summary>
        /// Gets or sets the BalancerVirtualMass.
        /// </summary>
        public abstract float BalancerVirtualMass { get; set; }
        /// <summary>
        /// Gets or sets the GridPosition.
        /// </summary>
        /// <summary>
        /// Gets or sets the GridPosition.
        /// </summary>
        public abstract Vector3I GridPosition { get; }
    }
}