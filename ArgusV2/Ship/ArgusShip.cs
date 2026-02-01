using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Helper.Log;
using IngameScript.Ship.Components;
using IngameScript.TruncationWrappers;
using VRageMath;

namespace IngameScript.Ship
{
    /// <summary>
    /// The base ship type.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public abstract class ArgusShip
    {
        protected AT_Vector3D CPreviousVelocity;
        protected AT_Vector3D CVelocity;
        protected int RandomUpdateJitter;
        public PollFrequency PollFrequency = PollFrequency.Realtime;

        public ArgusShip()
        {
            Program.LogLine("New ArgusShip", LogLevel.Debug);
            RandomUpdateJitter = Program.RNG.Next() % 6000;
        }
        
        public abstract AT_Vector3D Position { get; }
        /// <summary>
        /// Gets or sets the Velocity.
        /// </summary>
        /// <summary>
        /// Gets or sets the Velocity.
        /// </summary>
        public abstract AT_Vector3D Velocity { get; }
        /// <summary>
        /// Gets or sets the Acceleration.
        /// </summary>
        /// <summary>
        /// Gets or sets the Acceleration.
        /// </summary>
        public abstract AT_Vector3D Acceleration { get; }
        /// <summary>
        /// Gets or sets the GridSize.
        /// </summary>
        /// <summary>
        /// Gets or sets the GridSize.
        /// </summary>
        public abstract float GridSize { get; }
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public abstract string Name { get; }


        /// <summary>
        /// The first update to be called. This should be for grabbing new data from the API, e.g. positions, velocities, targets.
        /// </summary>
        /// <param name="frame"></param>
        /// <summary>
        /// EarlyUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// EarlyUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        public abstract void EarlyUpdate(int frame);
        
        /// <summary>
        /// The second update to be called. This would be for anything that depends on up-to-date data from other ships, e.g. ballistic calculations, distance checks.
        /// </summary>
        /// <param name="frame"></param>
        /// <summary>
        /// LateUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// LateUpdate method.
        /// </summary>
        /// <param name="frame">The frame parameter.</param>
        /// <returns>The result of the operation.</returns>
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
        /// <summary>
        /// GetTargetLeadPosition method.
        /// </summary>
        /// <param name="target">The target parameter.</param>
        /// <param name="projectileVelocity">The projectileVelocity parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetTargetLeadPosition method.
        /// </summary>
        /// <param name="target">The target parameter.</param>
        /// <param name="projectileVelocity">The projectileVelocity parameter.</param>
        /// <returns>The result of the operation.</returns>
        public AT_Vector3D GetTargetLeadPosition(ArgusShip target, float projectileVelocity)
        {
            AT_Vector3D shooterPos = this.Position;
            AT_Vector3D shooterVel = this.Velocity;
            
            AT_Vector3D targetPos = target.Position;
            AT_Vector3D targetAcc = target.Acceleration;

            AT_Vector3D relativeVel = target.Velocity - shooterVel; // target motion relative to shooter
            
            AT_Vector3D displacement = targetPos - shooterPos;
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
            AT_Vector3D intercept = targetPos + relativeVel * t + 0.5 * targetAcc * t * t;
            return intercept;
        }

        
        

        
    }
}