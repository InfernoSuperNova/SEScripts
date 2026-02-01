using System;
using IngameScript.TruncationWrappers;
using VRageMath;

namespace IngameScript.Helper
{
            #region Solver

            
       
        public static class Solver
        {
            public static double MaxValueD => Double.MaxValue;
            
            public const double
                epsilon = 1e-6,

                cos120d = -0.5,
                root3 = 1.73205,
                sin120d = root3 / 2,

                inv3 = 1.0 / 3.0,
                inv9 = 1.0 / 9.0,
                inv6 = 1.0 / 6.0,
                inv54 = 1.0 / 54.0;

            public static AT_Vector3D BallisticSolver(double maxSpeed, AT_Vector3D missileVelocity, AT_Vector3D missileAcceleration, AT_Vector3D displacementVector, AT_Vector3D targetPosition, AT_Vector3D targetVelocity, AT_Vector3D targetAcceleration, AT_Vector3D targetJerk, bool isMissile, AT_Vector3D gravity = default(AT_Vector3D), bool hasGravity = false)
            {
                double tmaxT = 0;
                AT_Vector3D
                    dOffset = AT_Vector3D.Zero,    
                    targetAccelerationS = targetAcceleration,
                    targetVelocityS = targetVelocity,
                    relativeVelocity, relativeAcceleration;

                if (targetAcceleration.LengthSquared() > 1)
                {
                    //Target Max Speed Math
                    tmaxT = Math.Min((AT_Vector3D.Normalize(targetAcceleration) * Config.General.GridSpeedLimit - AT_Vector3D.ProjectOnVector(ref targetVelocity, ref targetAcceleration)).Length(), 2 * Config.General.GridSpeedLimit) / targetAcceleration.Length();
                    targetVelocity = AT_Vector3D.ClampToSphere(targetVelocity + targetAcceleration * tmaxT, Config.General.GridSpeedLimit);
                    targetVelocityS += targetAcceleration * tmaxT * 0.5;
                    targetAcceleration = AT_Vector3D.Zero;
                }

                if (missileAcceleration.LengthSquared() > 1)
                {
                    double
                        Tmax = Math.Max((AT_Vector3D.Normalize(missileAcceleration) * maxSpeed - AT_Vector3D.ProjectOnVector(ref missileVelocity, ref missileAcceleration)).Length(), 0) / missileAcceleration.Length(),
                        Dmax = (missileVelocity * Tmax + missileAcceleration * Tmax * Tmax).Length();
                    AT_Vector3D posAtTmax = displacementVector + targetVelocity * Tmax + 0.5 * targetAcceleration * Tmax * Tmax;
                    if (posAtTmax.Length() > Dmax)
                    {
                        missileAcceleration = AT_Vector3D.Zero;
                        missileVelocity = AT_Vector3D.ClampToSphere(missileVelocity + missileAcceleration * Tmax, maxSpeed);
                        displacementVector -= (AT_Vector3D)AT_Vector3D.Normalize(displacementVector) * Dmax;
                    }
                }

                //Quartic
                relativeVelocity = targetVelocity - missileVelocity;
                relativeAcceleration = targetAcceleration - missileAcceleration;
                double time = Solve(
                        relativeAcceleration.LengthSquared() * 0.25,
                        relativeAcceleration.X * relativeVelocity.X + relativeAcceleration.Y * relativeVelocity.Y + relativeAcceleration.Z * relativeVelocity.Z,
                        relativeVelocity.LengthSquared() - missileVelocity.LengthSquared() + displacementVector.X * relativeAcceleration.X + displacementVector.Y * relativeAcceleration.Y + displacementVector.Z * relativeAcceleration.Z,
                        2 * (displacementVector.X * relativeVelocity.X + displacementVector.Y * relativeVelocity.Y + displacementVector.Z * relativeVelocity.Z),
                        displacementVector.LengthSquared());
                if (time == MaxValueD || double.IsNaN(time) || time > 100)
                    time = 100;

                if (tmaxT > time)
                {
                    tmaxT = time;
                    time = 0;
                }   
                else
                    time -= tmaxT;
                //sbDebug.AppendLine($"{Math.Round(time, 2)} {Math.Round(relativeVelocity.Length(), 2)} {Math.Round(relativeAcceleration.Length(), 2)}\n");
                return isMissile ?
                    displacementVector + targetVelocity * time + targetVelocityS * tmaxT + 0.5 * targetAccelerationS * tmaxT * tmaxT + 0.5 * targetAcceleration * time * time + dOffset :
                    targetPosition + (targetVelocity - missileVelocity) * time + (targetVelocityS - missileVelocity) * tmaxT + 0.5 * targetAccelerationS * tmaxT * tmaxT + 0.5 * targetAcceleration * time * time + - 0.5 * gravity * (time + tmaxT) * (time + tmaxT) * Convert.ToDouble(hasGravity) + dOffset;
            }
            // public static Vector3D SelectOffsetPosition(PDCTarget target, bool isMissile)
            // {
            //     int
            //         RaycastPointCount = target.RaycastPoints.Count,
            //         TurretPointCount = target.TurretPoints.Count;
            //
            //     if (RaycastPointCount + TurretPointCount == 0 || target.Orientation == null)
            //         return Vector3D.Zero;
            //
            //     if (isMissile)
            //         return RaycastPointCount != 0 ?
            //             target.RaycastPoints.ElementAt(Program.RNG.Next(0, RaycastPointCount)) :
            //             target.TurretPoints.ElementAt(Program.RNG.Next(0, TurretPointCount));
            //     else
            //         return TurretPointCount != 0 ?
            //             target.TurretPoints.ElementAt(Program.RNG.Next(0, TurretPointCount)) :
            //             target.RaycastPoints.ElementAt(Program.RNG.Next(0, RaycastPointCount));
            // }

            //Shortcut Ignoring Of Complex Values And Return Smallest Real Number
        /// <summary>
        /// Solve method.
        /// </summary>
        /// <param name="a">The a parameter.</param>
        /// <param name="b">The b parameter.</param>
        /// <param name="c">The c parameter.</param>
        /// <param name="d">The d parameter.</param>
        /// <param name="e">The e parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Solve method.
        /// </summary>
        /// <param name="a">The a parameter.</param>
        /// <param name="b">The b parameter.</param>
        /// <param name="c">The c parameter.</param>
        /// <param name="d">The d parameter.</param>
        /// <param name="e">The e parameter.</param>
        /// <returns>The result of the operation.</returns>
            public static double Solve(double a, double b, double c, double d, double e)
            {
                if (Math.Abs(a) < epsilon) a = a >= 0 ? epsilon : -epsilon;
                double inva = 1 / a;

                b *= inva;
                c *= inva;
                d *= inva;
                e *= inva;

                double
                    a3 = -c,
                    b3 = b * d - 4 * e,
                    c3 = -b * b * e - d * d + 4 * c * e,
                    y;

                double[] result;
                bool chooseMaximal = SolveCubic(a3, b3, c3, out result);
                y = result[0];
                if (chooseMaximal)
                {
                    if (Math.Abs(result[1]) > Math.Abs(y))
                        y = result[1];
                    if (Math.Abs(result[2]) > Math.Abs(y))
                        y = result[2];
                }

                double q1, q2, p1, p2, squ;

                double u = y * y - 4 * e;
                if (Math.Abs(u) < epsilon)
                {
                    q1 = q2 = y * 0.5;
                    u = b * b - 4 * (c - y);

                    if (Math.Abs(u) < epsilon)
                        p1 = p2 = b * 0.5;
                    else
                    {
                        squ = Math.Sqrt(u);
                        p1 = (b + squ) * 0.5;
                        p2 = (b - squ) * 0.5;
                    }
                }
                else
                {
                    squ = Math.Sqrt(u);
                    q1 = (y + squ) * 0.5;
                    q2 = (y - squ) * 0.5;

                    double dm = 1 / (q1 - q2);
                    p1 = (b * q1 - d) * dm;
                    p2 = (d - b * q2) * dm;
                }

                double v1, v2;

                u = p1 * p1 - 4 * q1;
                if (u < 0)
                    v1 = MaxValueD;
                else
                {
                    squ = Math.Sqrt(u);
                    v1 = MinPosNZ(-p1 + squ, -p1 - squ) * 0.5;
                }

                u = p2 * p2 - 4 * q2;
                if (u < 0)
                    v2 = MaxValueD;
                else
                {
                    squ = Math.Sqrt(u);
                    v2 = MinPosNZ(-p2 + squ, -p2 - squ) * 0.5;
                }

                return MinPosNZ(v1, v2);
            }

            private static bool SolveCubic(double a, double b, double c, out double[] result)
            {
                result = new double[4];

                double
                    a2 = a * a,
                    q = (a2 - 3 * b) * inv9,
                    r = (a * (2 * a2 - 9 * b) + 27 * c) * inv54,
                    r2 = r * r,
                    q3 = q * q * q;

                if (r2 < q3)
                {
                    double
                        sqq = Math.Sqrt(q),
                        t = r / (sqq * sqq * sqq);

                    if (t < -1)
                        t = -1;
                    else if (t > 1)
                        t = 1;

                    t = Math.Acos(t);

                    a *= inv3;
                    q = -2 * sqq;

                    double
                        costv3 = Math.Cos(t * inv3),
                        sintv3 = Math.Sin(t * inv3);

                    result[0] = q * costv3 - a;
                    result[1] = q * ((costv3 * cos120d) - (sintv3 * sin120d)) - a;
                    result[2] = q * ((costv3 * cos120d) + (sintv3 * sin120d)) - a;

                    return true;
                }
                else
                {
                    double
                        g = -Math.Pow(Math.Abs(r) + Math.Sqrt(r2 - q3), inv3),
                        h;

                    if (r < 0)
                        g = -g;

                    h = g == 0 ? 0 : q / g;

                    a *= inv3;

                    result[0] = g + h - a;
                    result[1] = -0.5 * (g + h) - a;
                    result[2] = 0.5 * root3 * (g - h);

                    if (Math.Abs(result[2]) < epsilon)
                    {
                        result[2] = result[1];
                        return true;
                    }
                    return false;
                }
            }

            private static double MinPosNZ(double a, double b)
            {
                if (a <= 0)
                    return b > 0 ? b : MaxValueD;
                else if (b <= 0)
                    return a;
                else
                    return Math.Min(a, b);
            }
        }
        #endregion
}