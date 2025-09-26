using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public class DroneIgcClient
    {
        public long Id;
        public int TimesMissedHeartbeat;
        public bool JustMissedHeartbeat;

        private readonly IMyIntergridCommunicationSystem _igc;
        public DroneIgcClient(long id, IMyIntergridCommunicationSystem igc)
        {
            Id = id;
            _igc = igc;
        }

        private Vector3 _direction;
        public Vector3 Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
                _igc.SendUnicastMessage(Id, SendKeys.DroneDirection, _direction);
            }
        }

        private float _distance;
        public float Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                _distance = value;
                _igc.SendUnicastMessage(Id, SendKeys.DroneDistance, _distance);
            }
        }

        public Vector3D MothershipPosition
        {
            set
            {
                _igc.SendUnicastMessage(Id, SendKeys.MothershipPosition, value);
            }
        }
        public Vector3 MothershipVelocity
        {
            set
            {
                _igc.SendUnicastMessage(Id, SendKeys.MothershipVelocity, value);
            }
        }
        public Vector3 MothershipDirection
        {
            set
            {
                _igc.SendUnicastMessage(Id, SendKeys.MothershipDirection, value);
            }
        }

        public Vector3D LastSeenPos;

        public void TakeOwnership()
        {
            _igc.SendUnicastMessage(Id, SendKeys.DroneTakeOwnership, _igc.Me);
        }
    }
}