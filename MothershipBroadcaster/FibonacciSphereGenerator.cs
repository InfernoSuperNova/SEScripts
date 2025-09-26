using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    public static class FibonacciSphereGenerator
    {
        static float phi = (float)(Math.PI * (3.0 - Math.Sqrt(5.0))); // golden ratio
        public static List<Vector3> GenerateDirections(int count)
        {
            
            List<Vector3> directions = new List<Vector3>();
            if (count <= 1)
            {
                directions.Add(Vector3.Forward);
                return directions;
            }
            for (int i = 0; i < count; i++)
            {
                float y = 1f - (i / (float)(count - 1)) * 2f; // from 1 to -1
                float radiusAtY = (float)Math.Sqrt(1 - y * y);
                float theta = phi * i;

                float x = (float)(Math.Cos(theta) * radiusAtY);
                float z = (float)(Math.Sin(theta) * radiusAtY);

                directions.Add(new Vector3D(x, y, z)); // already a unit vector
            }

            return directions;
        }
    }
}