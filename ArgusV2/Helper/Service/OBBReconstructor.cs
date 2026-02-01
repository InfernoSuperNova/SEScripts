using System;
using System.Collections.Generic;
using IngameScript.TruncationWrappers;
using VRageMath;

namespace IngameScript.Helper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public static class OBBReconstructor
    {
        
        /// <summary>
        /// Checks if a hypothetical OBB (defined by its integer grid extents)
        /// is fully contained within the world-space AABB using SAT.
        /// </summary>
        private static bool IsObbContained(
            long i1, long i2, long i3,
            double gridSize,
            MatrixD obbRotation,
            AT_Vector3D worldHalfExtents)
        {
            // 1. Calculate the OBB's continuous half-extents.
            double r1 = i1 * gridSize;
            double r2 = i2 * gridSize;
            double r3 = i3 * gridSize;
        
            // 2. Get the OBB's local axes and absolute components.
            AT_Vector3D u1 = obbRotation.Right;
            AT_Vector3D u2 = obbRotation.Up;
            AT_Vector3D u3 = obbRotation.Forward;
        
            AT_Vector3D abs_u1 = AT_Vector3D.Abs(u1);
            AT_Vector3D abs_u2 = AT_Vector3D.Abs(u2);
            AT_Vector3D abs_u3 = AT_Vector3D.Abs(u3);
        
            // 3. Perform the Separating Axis Theorem (SAT) check.
            // Check projection onto World X, Y, Z axes.
            
            // Robust Tolerance: Prevents premature pruning due to floating point inaccuracies 
            // when OBB projections are extremely close to the AABB boundaries (hard diagonals).
            const double RobustTolerance = 1e-8; 
        
            // Projected Radius onto World X-axis: R_x = r1|u1.X| + r2|u2.X| + r3|u3.X|
            double projectedRadiusX = r1 * abs_u1.X + r2 * abs_u2.X + r3 * abs_u3.X;
            if (projectedRadiusX > worldHalfExtents.X + RobustTolerance) return false;
        
            // Projected Radius onto World Y-axis: R_y = r1|u1.Y| + r2|u2.Y| + r3|u3.Y|
            double projectedRadiusY = r1 * abs_u1.Y + r2 * abs_u2.Y + r3 * abs_u3.Y;
            if (projectedRadiusY > worldHalfExtents.Y + RobustTolerance) return false;
        
            // Projected Radius onto World Z-axis: R_z = r1|u1.Z| + r2 * abs_u2.Z + r3 * abs_u3.Z;
            double projectedRadiusZ = r1 * abs_u1.Z + r2 * abs_u2.Z + r3 * abs_u3.Z;
            if (projectedRadiusZ > worldHalfExtents.Z + RobustTolerance) return false;
        
            return true;
        }
        
        // Helper: Try to solve A * r = H for r = [r1,r2,r3].
        // A_{row, col} = abs(u_col.{X,Y,Z})
        private static bool TrySolveContinuousHalfExtents(
            AT_Vector3D u1, AT_Vector3D u2, AT_Vector3D u3,
            AT_Vector3D worldHalfExtents,
            out double r1, out double r2, out double r3)
        {
            var a11 = Math.Abs(u1.X); var a12 = Math.Abs(u2.X); var a13 = Math.Abs(u3.X);
            var a21 = Math.Abs(u1.Y); var a22 = Math.Abs(u2.Y); var a23 = Math.Abs(u3.Y);
            var a31 = Math.Abs(u1.Z); var a32 = Math.Abs(u2.Z); var a33 = Math.Abs(u3.Z);

            double Hx = worldHalfExtents.X, Hy = worldHalfExtents.Y, Hz = worldHalfExtents.Z;

            // Determinant
            double det = a11 * (a22 * a33 - a23 * a32)
                       - a12 * (a21 * a33 - a23 * a31)
                       + a13 * (a21 * a32 - a22 * a31);

            const double EPS = 1e-12;
            if (Math.Abs(det) < EPS)
            {
                r1 = r2 = r3 = 0.0;
                return false;
            }

            // Inverse (adjugate / det)
            double inv11 =  (a22 * a33 - a23 * a32) / det;
            double inv12 = -(a12 * a33 - a13 * a32) / det;
            double inv13 =  (a12 * a23 - a13 * a22) / det;

            double inv21 = -(a21 * a33 - a23 * a31) / det;
            double inv22 =  (a11 * a33 - a13 * a31) / det;
            double inv23 = -(a11 * a23 - a13 * a21) / det;

            double inv31 =  (a21 * a32 - a22 * a31) / det;
            double inv32 = -(a11 * a32 - a12 * a31) / det;
            double inv33 =  (a11 * a22 - a12 * a21) / det;

            r1 = inv11 * Hx + inv12 * Hy + inv13 * Hz;
            r2 = inv21 * Hx + inv22 * Hy + inv23 * Hz;
            r3 = inv31 * Hx + inv32 * Hy + inv33 * Hz;

            // All radii must be non-negative (tiny negative due to FP -> clamp)
            const double NEG_CLAMP = -1e-9;
            if (r1 < NEG_CLAMP || r2 < NEG_CLAMP || r3 < NEG_CLAMP) return false;

            r1 = Math.Max(0.0, r1);
            r2 = Math.Max(0.0, r2);
            r3 = Math.Max(0.0, r3);

            return true;
        }

        public static BoundingBoxD GetMaxInscribedOBB(
            BoundingBoxD worldAabb,
            MatrixD obbRotation,
            double gridSize)
        {
            gridSize /= 2;
            
            AT_Vector3D worldHalfExtents = worldAabb.HalfExtents;
            AT_Vector3D u1 = obbRotation.Right;
            AT_Vector3D u2 = obbRotation.Up;
            AT_Vector3D u3 = obbRotation.Forward;

            // Try direct linear solve first (this will typically recover the original slender box)
            double r1c, r2c, r3c;
            if (TrySolveContinuousHalfExtents(u1, u2, u3, worldHalfExtents, out r1c, out r2c, out r3c))
            {
                long i1 = Math.Max(0, (long)(r1c / gridSize + 1e-12));
                long i2 = Math.Max(0, (long)(r2c / gridSize + 1e-12));
                long i3 = Math.Max(0, (long)(r3c / gridSize + 1e-12));

                
                // Greedily grow each axis by +1 while it still fits (reclaim integer slack). This helps eliminate weird edge cases
                bool changed;
                do
                {
                    changed = false;
                    if (IsObbContained(i1 + 1, i2, i3, gridSize, obbRotation, worldHalfExtents)) { i1++; changed = true; }
                    if (IsObbContained(i1, i2 + 1, i3, gridSize, obbRotation, worldHalfExtents)) { i2++; changed = true; }
                    if (IsObbContained(i1, i2, i3 + 1, gridSize, obbRotation, worldHalfExtents)) { i3++; changed = true; }
                } while (changed);
                
                
                var obbHalfExtents = new AT_Vector3D(i1 * gridSize, i2 * gridSize, i3 * gridSize);
                return new BoundingBoxD(-obbHalfExtents, obbHalfExtents);
            }
            
            double maxRadiusWorldX = worldHalfExtents.X / (Math.Abs(u1.X) + Math.Abs(u2.X) + Math.Abs(u3.X));
            double maxRadiusWorldY = worldHalfExtents.Y / (Math.Abs(u1.Y) + Math.Abs(u2.Y) + Math.Abs(u3.Y));
            double maxRadiusWorldZ = worldHalfExtents.Z / (Math.Abs(u1.Z) + Math.Abs(u2.Z) + Math.Abs(u3.Z));

            double maxFittingRadius = Math.Min(Math.Min(maxRadiusWorldX, maxRadiusWorldY), maxRadiusWorldZ);
            long I_max = Math.Max(0, (long)Math.Floor(maxFittingRadius / gridSize));
            
            var conservativeHalf = new AT_Vector3D(I_max * gridSize, I_max * gridSize, I_max * gridSize);
            return new BoundingBoxD(-conservativeHalf, conservativeHalf);
        }
    }
}





    
            