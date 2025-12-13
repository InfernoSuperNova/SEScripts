using System.Collections.Generic;
using IngameScript.TruncationWrappers;
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
        
        private LaunchRequest _launchRequest;
        

        public Missile(List<IMyCubeBlock> blocks, LaunchControl launchCapability)
        {
            _blocks = new List<IMyCubeBlock>(blocks);
            _gyro = blocks.Find(b => b is IMyGyro) as IMyGyro;
            LaunchCapability = launchCapability;
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

        // Used at this point to determine if a missile in flight is suitable to be redirected for CIWS duties
        public LaunchControl LaunchCapability { get; }
        
        /// <summary>
        /// How this missile was actually launched
        /// </summary>
        public LaunchControl LaunchContext => _launchRequest.LaunchControl;
        public AT_Vector3D Position => _gyro.GetPosition();

        public void SetLaunchParameters(LaunchRequest launchRequest)
        {
            _launchRequest = launchRequest;
        }

        public void EarlyUpdate(int frame)
        {

        }

        public void LateUpdate(int frame)
        {
            
        }

        public void Redirect(TrackableShip target, LaunchRequest launchRequest)
        {
            _launchRequest = launchRequest;
            // TODO: Set target
        }
    }
}