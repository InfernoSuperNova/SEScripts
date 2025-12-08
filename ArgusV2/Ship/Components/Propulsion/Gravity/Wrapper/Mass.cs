using IngameScript.TruncationWrappers;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public abstract class Mass
    {
        public abstract AT_Vector3D Position { get; }
        public abstract double AbsoluteVirtualMass { get; set; }
        public abstract float BalancerVirtualMass { get; set; }
        public abstract Vector3I GridPosition { get; }
    }
}