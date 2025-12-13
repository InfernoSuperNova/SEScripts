using System;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public static class Targeting
    {
        private static double maxVelocity = 104;
        public static IMyShipController Controller;
        public static MyGridProgram program;

        public static Vector3D ComputeInterceptPoint(ref Vector3D targetPosition, Vector3D targetVelocity, Vector3D targetAcceleration, ref Vector3D shipPosition, ref Vector3D shipVelocity, ref Vector3D gravity, ref long lastTargetEntityId, WeaponDefinition weapon)
            {
                Vector3D displacementVector = targetPosition - shipPosition;
        
                double Tmax = 0;
                double Dmax = 0;
                double DAtTmax = 0;
                double TApprox = displacementVector.Length() / (Vector3D.Normalize(displacementVector) * weapon.MaxSpeed + (targetVelocity - shipVelocity)).Length();
                double speedApprox = (Vector3D.Normalize(targetPosition + targetVelocity * TApprox + targetAcceleration * TApprox * TApprox - shipPosition) * weapon.InitialSpeed + shipVelocity).Length();
                if (weapon.IsCappedSpeed)
                    speedApprox = Math.Min(speedApprox, weapon.MaxSpeed);
        
                if (weapon.Acceleration != 0)
                {
                    Tmax = (weapon.MaxSpeed - speedApprox) / weapon.Acceleration;
        
                    Dmax =weapon.InitialSpeed * Tmax + weapon.Acceleration * Tmax * Tmax;
                    DAtTmax = (targetPosition + targetVelocity * Tmax + 0.5 * targetAcceleration * Tmax * Tmax - shipPosition).Length();
                }
        
                Vector3D TargetVelocityS = targetVelocity;
                double MissileAccelerationD = weapon.Acceleration;
                double MissileVelocityD = speedApprox;
                double TargetAccelerationD = targetAcceleration.Length();
                double TargetVelocityD = targetVelocity.Length();
                double TOffset = 0;
                Vector3D DOffset = Vector3D.Zero;
                if (DAtTmax > Dmax)
                {
                    MissileAccelerationD = 0;
                    MissileVelocityD = weapon.MaxSpeed;
                    DOffset = -Vector3D.Normalize(displacementVector) * Dmax;
                    TOffset = Tmax;
                }
        
                //Target Max Speed Math
                double TmaxT = 0;
                if (TargetAccelerationD > 1)
                {
                    TmaxT = (maxVelocity - Math.Min(Vector3D.ProjectOnVector(ref targetVelocity, ref targetAcceleration).Length(), maxVelocity)) / TargetAccelerationD;
                    TargetAccelerationD = 0;
                    targetVelocity = Vector3D.Normalize(targetVelocity) * maxVelocity;
                    TargetVelocityD = maxVelocity;
                }
        
                //Quartic
                double a = (0.25 * (MissileAccelerationD * MissileAccelerationD)) - (0.25 * (TargetAccelerationD * TargetAccelerationD));
                double b = (MissileVelocityD * MissileAccelerationD) - (TargetVelocityD * TargetAccelerationD);
                double c = (MissileVelocityD * MissileVelocityD) - (TargetVelocityD * TargetVelocityD) - displacementVector.Dot(targetAcceleration);
                double d = -2 * targetVelocity.Dot(displacementVector);
                double e = -displacementVector.LengthSquared();
        
                //double time = FastSolver.Solve(a, b, c, d, e) - TOffset;
                double time = 0;
                if (time == double.MaxValue || double.IsNaN(time))
                    time = 100;
        
                if (TmaxT > time)
                {
                    TmaxT = time;
                    time = 0;
                }
                else
                    time -= TmaxT;
        
                return targetPosition + (targetVelocity - shipVelocity) * time + (TargetVelocityS - shipVelocity) * TmaxT + 0.5 * targetAcceleration * TmaxT * TmaxT - 0.5 * gravity * (time + TmaxT) * (time + TmaxT) * Convert.ToDouble(weapon.HasGravity) + DOffset;
            }
        
        public static Vector3D GetTargetLeadPosition(Vector3D targetPos, Vector3D targetVel, Vector3D shooterPos,
            Vector3D shooterVel, float projectileSpeed, double timeStep, ref Vector3D previousTargetVelocity,
            bool doEcho, bool leadAcceleration)
        {
            var deltaV = (targetVel - previousTargetVelocity) / timeStep / 2;

            var relativePos = targetPos - shooterPos;
            var relativeVel = targetVel - shooterVel;
            var gravity = Controller.GetNaturalGravity() / 2;

            deltaV = leadAcceleration ? deltaV : Vector3D.Zero;

            var timeToIntercept = CalculateTimeToIntercept(relativePos, relativeVel, deltaV, -gravity, projectileSpeed);
            var targetLeadPos = targetPos + (deltaV + relativeVel) * timeToIntercept + -gravity * timeToIntercept;

            previousTargetVelocity = targetVel;
            return targetLeadPos;
        }

        private static Vector3D RotatePosition(Vector3D position, Vector3D center, MatrixD rotation)
        {
            var relativePosition = position - center;
            var rotatedPosition = Vector3D.Transform(relativePosition, rotation);
            return center + rotatedPosition;
        }

        private static double CalculateTimeToIntercept(Vector3D relativePos, Vector3D relativeVel, Vector3D targetAcc,
            Vector3D gravity, float projectileSpeed)
        {
            var a = targetAcc.ALengthSquared() - projectileSpeed * projectileSpeed;
            var b = 2 * (Vector3D.Dot(relativePos, targetAcc) + Vector3D.Dot(relativeVel, targetAcc));
            var c = relativePos.ALengthSquared();

            // Factor in gravity
            a += gravity.ALengthSquared();
            b += 2 * Vector3D.Dot(relativePos, gravity);

            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
                // No real solution, return a default position (e.g., current target position)
                return 0;

            var t1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            var t2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

            if (t1 < 0 && t2 < 0)
                // Both solutions are negative, return a default position (e.g., current target position)
                return 0;
            if (t1 < 0)
                // t1 is negative, return t2
                return t2;
            if (t2 < 0)
                // t2 is negative, return t1
                return t1;
            // Both solutions are valid, return the minimum positive time
            return Math.Min(t1, t2);
        }
    }
}