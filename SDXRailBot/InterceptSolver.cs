using System;
using System.Collections.Generic;
using VRageMath; // Space Engineers API for Vector3D

public static class InterceptSolver
{
    /// <summary>
    /// Computes the intercept point of a projectile fired from shooterPos to hit a target moving with initial
    /// position targetPos, velocity targetVel, and acceleration targetAcc, given projectileSpeed.
    /// Solves a quartic equation analytically and returns the first positive real solution.
    /// Returns null if no valid intercept time is found.
    /// </summary>
    public static Vector3D Compute(Vector3D targetLocation, Vector3D targetVelocity, Vector3D targetAcceleration,
        Vector3D projectileLocation, double projectileInitialSpeed, double projectileAcceleration,
        double projectileMaxSpeed)
    {
//---------- Calculate Impact Point ----------

//targetDirection Must Be Normalized
        Vector3D z = targetLocation - projectileLocation;
        double k = (projectileAcceleration == 0
            ? 0
            : (projectileMaxSpeed - projectileInitialSpeed) / projectileAcceleration);
        double p = (0.5 * projectileAcceleration * k * k) + (projectileInitialSpeed * k) - (projectileMaxSpeed * k);

        double a = (projectileMaxSpeed * projectileMaxSpeed) - targetVelocity.LengthSquared();
        double b = 2 * ((p * projectileMaxSpeed) - targetVelocity.Dot(z));
        double c = (p * p) - z.LengthSquared();

        double t = SolveQuadratic(a, b, c);

        if (Double.IsNaN(t) || t < 0)
        {
            return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
        }
        else
        {
            if (targetAcceleration.Sum > 0.001)
            {
                return targetLocation + (targetVelocity * t) + (0.5 * targetAcceleration * t * t);
            }
            else
            {
                return targetLocation + (targetVelocity * t);
            }
        }
    }

    static double SolveQuadratic(double a, double b, double c)
    {
        double u = (b * b) - (4 * a * c);
        if (u < 0)
        {
            return Double.NaN;
        }

        u = Math.Sqrt(u);

        double t1 = (-b + u) / (2 * a);
        double t2 = (-b - u) / (2 * a);
        return (t1 > 0 ? (t2 > 0 ? (t1 < t2 ? t1 : t2) : t1) : t2);
    }
}