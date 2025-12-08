using System;
using System.Collections.Generic;
using System.Linq;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Helper
{
    public class OrientedOccludedSphere : OccludedSphere
    {
        
        public OrientedOccludedSphere(MatrixD matrix, double radius, Dictionary<int, Bounds> constraints) : base (radius, constraints)
        {
            Matrix = MatrixD.Invert(matrix);
        }

        public MatrixD Matrix { get; set; }

        public override bool Intersects(AT_Vector3D point)
        {
            
            
            
            point = AT_Vector3D.Transform(point, Matrix);
            return base.Intersects(point);
        }
        
        
    }
    
    
    
    /// <summary>
    /// Assumed to be axis aligned.
    /// </summary>
    public class OccludedSphere
    {
        private Dictionary<int, Bounds> _lookup;
        protected double Radius;

        public OccludedSphere( double radius, Dictionary<int, Bounds> constraints)
        {
            IMyProjector proj;
            Radius = radius;
            
            
            var keys = constraints.Keys.OrderBy(k => k).ToList();
            _lookup = new Dictionary<int, Bounds>();

            for (int i = 0; i < keys.Count; i++)
            {
                int startKey = keys[i];
                int endKey = keys[(i + 1) % keys.Count]; // wrap around for 359→0
                var startBounds = constraints[startKey];
                var endBounds = constraints[endKey];

                int range = (endKey > startKey) ? endKey - startKey : (360 - startKey + endKey);

                for (int j = 0; j <= range; j++)
                {
                    int angle = (startKey + j) % 360;

                    double t = (double)j / range; // normalized [0,1]

                    // linear interpolation for min/max
                    double min = startBounds.Min + (endBounds.Min - startBounds.Min) * t;
                    double max = startBounds.Max + (endBounds.Max - startBounds.Max) * t;

                    _lookup[angle] = new Bounds((float)min, (float)max);
                }
            }
        }

        public Bounds Lookup(float azimuth)
        {
            var range = azimuth % 360;

            var floor = (int)Math.Floor(range);
            var ceil = (int)Math.Ceiling(range);

            var lower = _lookup[floor];
            var upper = _lookup[ceil];

            return Bounds.Lerp(lower, upper, range - floor);
        }
        /// <summary>
        /// Whether a given world point intersects with the non occluded geometry of this sphere.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public virtual bool Intersects(AT_Vector3D point)
        {
            var to = point;
            if (to.LengthSquared() > Radius * Radius) return false;
            
            var azimuth = Math.Atan2(to.Z, to.X) * (180f / Math.PI); // in degrees
            if (azimuth < 0) azimuth += 360;
            var bounds = Lookup((float)azimuth);
            var elevation = Math.Atan2(to.Y, Math.Sqrt(to.X * to.X + to.Z * to.Z)) * (180f / Math.PI);
            
            return elevation >= bounds.Min && elevation <= bounds.Max;
        }
        
        public struct Bounds
        {
            internal float Min;
            internal float Max;

            public Bounds(float min = 0, float max = 0)
            {
                Min = MathHelper.Clamp(min, -90, 90);
                Max = MathHelper.Clamp(max, min, 90);
            }

            public static Bounds Lerp(Bounds first, Bounds second, float t)
            {
                var min = MathHelper.Lerp(first.Min, second.Min, t);
                var max = MathHelper.Lerp(first.Max, second.Max, t);

                return new Bounds(min, max);
            }
        }
    }
}