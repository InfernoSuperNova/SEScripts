using IngameScript.Helper;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity.Wrapper
{
    /// <summary>
    /// GravityGeneratorSpherical class.
    /// </summary>
    /// <summary>
    /// GravityGeneratorSpherical class.
    /// </summary>
    public class GravityGeneratorSpherical : GravityGenerator
    {

        public GravityGeneratorSpherical(IMyGravityGeneratorSphere sphericalGen, Direction dir, bool inverted)
        {
            Generator = sphericalGen;
            Direction = dir;
            IsInverted = inverted;
        }
    }
}