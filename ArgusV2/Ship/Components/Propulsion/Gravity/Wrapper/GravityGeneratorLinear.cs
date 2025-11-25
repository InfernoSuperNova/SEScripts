using IngameScript.Helper;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    public class GravityGeneratorLinear : GravityGenerator
    {

        public GravityGeneratorLinear(IMyGravityGenerator gen, Direction dir, bool inverted) 
        {
            
            Generator = gen; 
            Direction = dir;
            IsInverted = inverted;
        }
    }
}