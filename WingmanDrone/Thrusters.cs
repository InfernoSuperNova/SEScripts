using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public static class Thrusters
    {
        public static List<IMyThrust> thrusters = new List<IMyThrust>();

        public static void Apply(Vector3D thrust)
        {
            foreach (var thruster in thrusters)
            {
                float power = (float)thruster.WorldMatrix.Forward.Dot(thrust);
                thruster.ThrustOverride = power * 100000;
            }
        }

        public static void Reset()
        {
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverride = 0;
            }
        }
    }
}