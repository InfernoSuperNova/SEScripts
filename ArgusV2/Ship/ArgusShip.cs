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
        /// The first update to be called. This only updates values within the grid and should not be used for extra grid activities.
        /// </summary>
        /// <param name="frame"></param>
        public abstract void EarlyUpdate(int frame);
        public Vector3D GetBallisticSolutionToShip(ArgusShip target, float projectileVelocity)
        {
            return Vector3D.Zero; // TODO
        }
        

    }
}