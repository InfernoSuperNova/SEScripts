using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class BallMass : Mass
    {
        private IMySpaceBall _ball;
        
        public override double VirtualMass => _ball.VirtualMass;
        
        
        public override bool BEnabled { get; set; }
        public override double BVirtualMass => BEnabled ? _ball.VirtualMass : 0;
    }
}