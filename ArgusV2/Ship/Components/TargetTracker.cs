using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship.Components
{
    public class TargetTracker
    {
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
        public bool JustLostTarget { get; private set; }
        public bool Invalid { get; private set; }
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
        public TrackableShip TrackedShip { get; set; }
        
        
        public AT_Vector3D Position => Block.GetPosition();

        /// <summary>Updates the HasTarget / JustLostTarget / TargetedEntity state.</summary>
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
        public AT_DetectedEntityInfo GetTargetedEntity()
        {
            var info = Block.GetTargetedEntity();
            TargetedEntity = info.EntityId;
            return info;
        }
    }
}