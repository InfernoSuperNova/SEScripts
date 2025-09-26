using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal class LinearGravityGenerator
    {
        public IMyGravityGenerator actualGravityGenerator;


        private readonly sbyte sign = 1;

        public LinearGravityGenerator(IMyGravityGenerator gravityGenerator, sbyte sign, string name)
        {
            actualGravityGenerator = gravityGenerator;
            this.sign = sign;
            gravityGenerator.CustomName = $"LGravity Generator [{name}]";
        }

        public void SetGravity(float gravity)
        {
            actualGravityGenerator.GravityAcceleration = gravity * sign * 9.81f;
        }
    }
}