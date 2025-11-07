using System;
using System.Collections.Generic;
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
        
        public abstract Vector3D Position { get; }
        public abstract Vector3D Velocity { get; }
        public abstract Vector3D Acceleration { get; }
        


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
        public Vector3D GetBallisticSolutionToShip(ArgusShip target, float projectileVelocity)
        {
            return Vector3D.Zero; // TODO
        }


        
    }
}