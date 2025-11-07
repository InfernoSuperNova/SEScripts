using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Ship
{
    /// <summary>
    /// Represents an enemy ship that can be tracked by Argus but cannot be controlled by Argus.
    /// </summary>
    public class TrackableShip : ArgusShip
    {
        private List<Vector3I> _all = new List<Vector3I>();
        private List<Vector3I> _weapons = new List<Vector3I>();
        private List<Vector3I> _propulsion = new List<Vector3I>();
        private List<Vector3I> _powerSystems = new List<Vector3I>();
        
        
        public SupportingShip TrackingShip;
        public IMyTurretControlBlock Tracker;

        public MyDetectedEntityInfo Info;

        public TrackableShip(IMyTurretControlBlock _tracker)
        {
            Tracker = _tracker;
        }

        public override Vector3D Position => Info.Position;
        public override Vector3D Velocity => CVelocity;
        public override Vector3D Acceleration => CVelocity - CPreviousVelocity;

        public override void EarlyUpdate(int frame)
        {
            Info = Tracker.GetTargetedEntity();
            CPreviousVelocity = CVelocity;
            CVelocity = Info.Velocity;
        }

        public override void LateUpdate(int frame)
        {
            
        }
        public void AddTrackedBlock(MyDetectedEntityInfo info, IMyLargeTurretBase turret = null, IMyTurretControlBlock controller = null)
        {
            if (info.EntityId != Info.EntityId || info.HitPosition == null) return;
            
            var targetBlockPosition = (Vector3D)info.HitPosition;
            var worldDirection = targetBlockPosition - Position;
            var positionInGridDouble = Vector3D.TransformNormal(worldDirection, MatrixD.Transpose(Info.Orientation)); // Should probably do testing here to ensure that this particular position is accurate
            var positionInGridInt = new Vector3I(
                (int)((positionInGridDouble.X + 0.5) / 2.5),
                (int)((positionInGridDouble.Y + 0.5) / 2.5),
                (int)((positionInGridDouble.Z + 0.5) / 2.5)
            );
            var targetingGroup = turret != null ? turret.GetTargetingGroup() : controller != null ? controller.GetTargetingGroup() : "";
            _all.Add(positionInGridInt);
            switch (targetingGroup)
            {
                case "Weapons":
                    _weapons.Add(positionInGridInt);
                    break;
                case "Propulsion":
                    _propulsion.Add(positionInGridInt);
                    break;
                case "Power Systems":
                    _powerSystems.Add(positionInGridInt);
                    break;
            }
        }
    }
}