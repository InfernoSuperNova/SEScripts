using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components;
using VRageMath;

namespace IngameScript.Ship
{
    /// <summary>
    /// The base ship type.
    /// </summary>
    public abstract class ArgusShip
    {
        protected Vector3D CPreviousVelocity;
        protected Vector3D CVelocity;
        protected int RandomUpdateJitter;
        public PollFrequency PollFrequency = PollFrequency.Realtime;

        public ArgusShip()
        {
            Program.LogLine("New ArgusShip", LogLevel.Debug);
            RandomUpdateJitter = Program.RNG.Next() % 6000;
        }
        
        public abstract Vector3D Position { get; }
        public abstract Vector3D Velocity { get; }
        public abstract Vector3D Acceleration { get; }
        public abstract float GridSize { get; }
        public abstract string Name { get; }


        /// <summary>
        /// The first update to be called. This should be for grabbing new data from the API, e.g. positions, velocities, targets.
        /// </summary>
        /// <param name="frame"></param>
        public abstract void EarlyUpdate(int frame);
        
        /// <summary>
        /// The second update to be called. This would be for anything that depends on up-to-date data from other ships, e.g. ballistic calculations, distance checks.
        /// </summary>
        /// <param name="frame"></param>
        public abstract void LateUpdate(int frame);
        
        
        /// <summary>
        /// Calculates a direction to aim in order to hit the specified target ship with a projectile
        /// traveling at the given velocity. 
        /// 
        /// WARNING: Should only be called during LateUpdate, when target and shooter velocities are up to date.
        /// Returns a vector pointing from this ship toward the predicted intercept point.
        /// </summary>
        /// <param name="target">The target ship to hit.</param>
        /// <param name="projectileVelocity">The speed of the projectile.</param>
        /// <returns>A normalized direction vector to aim at for impact.</returns>
        public Vector3D GetTargetLeadPosition(ArgusShip target, float projectileVelocity)
        {
            Vector3D shooterPos = this.Position;
            Vector3D shooterVel = this.Velocity;
            
            Vector3D targetPos = target.Position;
            Vector3D targetAcc = target.Acceleration;

            Vector3D relativeVel = target.Velocity - shooterVel; // target motion relative to shooter
            
            Vector3D displacement = targetPos - shooterPos;
            double s = projectileVelocity;

            // Quadratic coefficients: a t^2 + b t + c = 0
            double a = relativeVel.LengthSquared() - s * s;
            double b = 2.0 * displacement.Dot(relativeVel);
            double c = displacement.LengthSquared();

            double t;

            // Solve quadratic
            if (Math.Abs(a) < 1e-6) // handle edge case: target velocity ~= projectile speed
            {
                if (Math.Abs(b) < 1e-6)
                    t = 0; // target on top of shooter
                else
                    t = -c / b;
            }
            else
            {
                double discriminant = b * b - 4 * a * c;
                if (discriminant < 0) return targetPos; // cannot reach target, aim at current pos

                double sqrtDisc = Math.Sqrt(discriminant);
                double t1 = (-b + sqrtDisc) / (2 * a);
                double t2 = (-b - sqrtDisc) / (2 * a);

                t = Math.Min(t1, t2) > 0 ? Math.Min(t1, t2) : Math.Max(t1, t2);
                if (t < 0) t = Math.Max(t1, t2); // if both negative, aim at current pos
            }

            // Include acceleration via a single-step approximation
            Vector3D intercept = targetPos + relativeVel * t + 0.5 * targetAcc * t * t;
            return intercept;
        }

        
        

        
    }
}