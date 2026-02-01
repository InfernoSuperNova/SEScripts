using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public class TargetTracker
    {
        /// <summary>
        /// TargetTracker method.
        /// </summary>
        /// <param name="block">The block parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// TargetTracker method.
        /// </summary>
        /// <param name="block">The block parameter.</param>
        /// <returns>The result of the operation.</returns>
        public TargetTracker(IMyTurretControlBlock block)
        {
            Block = block;
        }

        public IMyTurretControlBlock Block { get; }
        public bool Closed => Block.Closed;

        // Explicit state tracking
        private bool _hadTarget;

        private bool _wasValid = true;

        public bool HasTarget { get; private set; }
        /// <summary>
        /// Gets or sets the JustLostTarget.
        /// </summary>
        /// <summary>
        /// Gets or sets the JustLostTarget.
        /// </summary>
        public bool JustLostTarget { get; private set; }
        /// <summary>
        /// Gets or sets the Invalid.
        /// </summary>
        /// <summary>
        /// Gets or sets the Invalid.
        /// </summary>
        public bool Invalid { get; private set; }
        /// <summary>
        /// Gets or sets the JustInvalidated.
        /// </summary>
        /// <summary>
        /// Gets or sets the JustInvalidated.
        /// </summary>
        public bool JustInvalidated { get; private set; }

        public bool Enabled
        {
            get { return Block.Enabled; }
            set { Block.Enabled = value; }
        }

        public string CustomName
        {
            get { return Block.CustomName; }
            set { Block.CustomName = value; }
        }

        public long TargetedEntity { get; set; }
        /// <summary>
        /// Gets or sets the TrackedShip.
        /// </summary>
        /// <summary>
        /// Gets or sets the TrackedShip.
        /// </summary>
        public TrackableShip TrackedShip { get; set; }
        
        
        public AT_Vector3D Position => Block.GetPosition();

        /// <summary>Updates the HasTarget / JustLostTarget / TargetedEntity state.</summary>
        /// <summary>
        /// UpdateState method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// UpdateState method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public void UpdateState()
        {
            var currentHasTarget = Block.HasTarget;
            JustLostTarget = !currentHasTarget && _hadTarget;
            _hadTarget = currentHasTarget;

            HasTarget = currentHasTarget;

            var currentInvalid = TrackedShip != null && TrackedShip.IntersectsLargerShipAABB;
            JustInvalidated = currentInvalid && _wasValid;
            _wasValid = !currentInvalid;

            Invalid = currentInvalid; 
            

            if (!currentHasTarget)
                TargetedEntity = 0;
        }

        /// <summary>Gets the currently targeted entity and updates TargetedEntity.</summary>
        /// <summary>
        /// GetTargetedEntity method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// GetTargetedEntity method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public AT_DetectedEntityInfo GetTargetedEntity()
        {
            var info = Block.GetTargetedEntity();
            TargetedEntity = info.EntityId;
            return info;
        }
    }
}