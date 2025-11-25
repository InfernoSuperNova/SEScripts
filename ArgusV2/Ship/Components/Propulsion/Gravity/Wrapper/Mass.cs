using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public abstract class Mass
    {
        public abstract bool BEnabled { get; set; }
        
        public abstract double VirtualMass { get; }
        public abstract double BVirtualMass { get; }
    }
}