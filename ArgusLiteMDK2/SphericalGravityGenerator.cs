using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal class SphericalGravityGenerator
    {
        public IMyGravityGeneratorSphere actualGravityGenerator;


        public sbyte sign = 1;

        public SphericalGravityGenerator(IMyGravityGeneratorSphere gravityGenerator, sbyte sign, string name)
        {
            actualGravityGenerator = gravityGenerator;
            this.sign = sign;
            gravityGenerator.CustomName = $"SGravity Generator [{name}]";
        }

        public void SetGravity(float gravity)
        {
            actualGravityGenerator.GravityAcceleration = gravity * sign * 9.81f;
        }

        public void SetRadius(float range)
        {
            actualGravityGenerator.Radius = range;
        }

        public void SetGravityOverride(float gravity)
        {
            actualGravityGenerator.GravityAcceleration = gravity;
        }
    }
}