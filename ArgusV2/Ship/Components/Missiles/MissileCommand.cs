using IngameScript.Ship.Components.Missiles.GuidanceObjective;

namespace IngameScript.Ship.Components.Missiles
{
    public struct MissileCommand
    {
        public MissileCommandContext MissileCommandContext { get; }
        //public FuseProfile FuseProfile { get; }
        public IMissionBehavior MissionBehavior { get; }

        public MissileCommand(
            MissileCommandContext missileCommandContext,
            IMissionBehavior missionBehavior
            //FuseProfile fuseProfile = FuseProfile.Default
            )
            
        {
            MissileCommandContext = missileCommandContext;
            MissionBehavior = missionBehavior;
            //FuseProfile = fuseProfile;
        }
    }
}
