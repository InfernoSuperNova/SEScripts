using System.Collections.Generic;
using IngameScript.Ship.Components.Propulsion.Gravity.Wrapper;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Propulsion.Gravity
{
    internal class BalancedMass
    {
        private List<BlockMass> blocks; // Ehhh should probably wrap the mass blocks too
        private List<BallMass> balls;
    }
}