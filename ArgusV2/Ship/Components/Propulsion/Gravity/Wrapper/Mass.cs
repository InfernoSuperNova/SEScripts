using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public abstract class Mass
    {
        
        public abstract double AbsoluteVirtualMass { get; }
        public abstract double BalancerVirtualMass { get; }
    }
}