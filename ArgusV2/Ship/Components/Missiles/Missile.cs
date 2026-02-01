using System.Collections.Generic;
using System.Linq;
using IngameScript.Ship.Components.Missiles.GuidanceObjective;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components.Missiles.LaunchMechanisms
{
    /// <summary>
    /// Represents a single missile with all its constituent blocks
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class Missile
    {
        private readonly List<IMyCubeBlock> _blocks;
        
        private readonly IMyGyro _gyro;
        
        private MissileCommand _missileCommand;
        
        private IMissionBehavior _missionBehavior;
        
        private List<IMyThrust> _thrusters; // TODO: Use thruster drive class

        public Missile(List<IMyCubeBlock> blocks, MissileCommandContext launchCapability)
        {
            _blocks = new List<IMyCubeBlock>(blocks);
            _gyro = blocks.Find(b => b is IMyGyro) as IMyGyro;
            LaunchCapability = launchCapability;
        }

        /// <summary>
        /// Gets all blocks that make up this missile
        /// </summary>
        public List<IMyCubeBlock> Blocks => _blocks;
        
        public int BlockCount => _blocks.Count;

        public IMyCubeGrid ReferenceGrid => _gyro.CubeGrid;

        // Used at this point to determine if a missile in flight is suitable to be redirected for CIWS duties
        public MissileCommandContext LaunchCapability { get; }
        
        public MissileCommandContext LaunchContext => _missileCommand.MissileCommandContext;
        public AT_Vector3D Position => _gyro.GetPosition();
        public Vector3D Forward => _thrusters[0].WorldMatrix.Forward;
        public Vector3D Up => _thrusters[0].WorldMatrix.Up;

        private void SetLaunchParameters(MissileCommand missileCommand)
        {
            _missileCommand = missileCommand;
            _missionBehavior = missileCommand.MissionBehavior;
            _missionBehavior.BindToMissile(this);
 
        }

        public void EarlyUpdate(int frame)
        {
            // At this point, we are away from all responsibilities and the missile just needs to handle these:
            // #1, Breakaway
            // #2, Cruise
            // #3, Terminal
            // #4, Impact and cleanup
            
            
            var command = _missionBehavior.Evaluate();
            
            
            var thrusters = new List<IMyThrust>(_blocks.FindAll(b => b is IMyThrust).Cast<IMyThrust>());
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverridePercentage = 1.0f;
            }
        }

        public void LateUpdate(int frame)
        {
            
        }

        public void Command(MissileCommand missileCommand)
        {
            SetLaunchParameters(missileCommand);
        }
    }
}