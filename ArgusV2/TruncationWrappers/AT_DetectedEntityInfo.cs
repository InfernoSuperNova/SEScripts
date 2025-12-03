using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRageMath;

namespace IngameScript.TruncationWrappers
{
    public struct AT_DetectedEntityInfo
    {
        private MyDetectedEntityInfo _info;
            AT_DetectedEntityInfo(MyDetectedEntityInfo info)
        {
            _info = info;
        }
        public static implicit operator AT_DetectedEntityInfo(MyDetectedEntityInfo info) => new AT_DetectedEntityInfo(info);
        public long EntityId => _info.EntityId;
        //public string Name => _info.Name;
        public MyDetectedEntityType Type => _info.Type;
        public Vector3D? HitPosition => _info.HitPosition;
        public MatrixD Orientation => _info.Orientation;
        public Vector3 Velocity => _info.Velocity;
        //public MyRelationsBetweenPlayerAndBlock Relationship => _info.Relationship;
        public BoundingBoxD BoundingBox => _info.BoundingBox;
        //public long TimeStamp => _info.TimeStamp;
        public Vector3D Position => BoundingBox.Center;

        

    }
}