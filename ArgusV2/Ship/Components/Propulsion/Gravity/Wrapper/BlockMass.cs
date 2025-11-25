namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BlockMass : Mass
    {
        public override bool BEnabled { get; set; }
        public override double VirtualMass { get; }
        public override double BVirtualMass { get; }
    }
}