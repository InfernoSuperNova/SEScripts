using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Ship.Components.Missiles.LaunchMechanisms
{
    /// <summary>
    /// Represents a single missile with all its constituent blocks
    /// </summary>
    public class Missile
    {
        private readonly List<IMyCubeBlock> _blocks;
        
        private readonly IMyGyro _gyro;

        public Missile(List<IMyCubeBlock> blocks)
        {
            _blocks = new List<IMyCubeBlock>(blocks);
            _gyro = blocks.Find(b => b is IMyGyro) as IMyGyro;
        }

        /// <summary>
        /// Gets all blocks that make up this missile
        /// </summary>
        public List<IMyCubeBlock> Blocks => _blocks;

        /// <summary>
        /// Gets the number of blocks in this missile
        /// </summary>
        public int BlockCount => _blocks.Count;

        public IMyCubeGrid ReferenceGrid => _gyro.CubeGrid;
    }
}