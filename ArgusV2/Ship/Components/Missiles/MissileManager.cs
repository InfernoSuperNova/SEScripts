using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles
{
    public class MissileManager
    {

        private List<MissileFinder> _missileFinders;


        public MissileManager(List<IMyTerminalBlock> blocks, ControllableShip ship)
        {
            
            _missileFinders = new List<MissileFinder>();
            foreach (var block in blocks)
            {
                if (block.CustomName.StartsWith(Config.String.MissileFinderPrefix)) _missileFinders.Add(new MissileFinder(block, ship));
            }
        }

        public void EarlyUpdate(int frame)
        {
            foreach (var finder in _missileFinders) finder.EarlyUpdate(frame);
        }

        public void LateUpdate(int frame)
        {
            foreach (var finder in _missileFinders) finder.LateUpdate(frame);
        }
    }
}