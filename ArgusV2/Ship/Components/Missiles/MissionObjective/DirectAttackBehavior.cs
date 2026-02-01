using IngameScript.Ship.Components.Missiles.LaunchMechanisms;
using IngameScript.TruncationWrappers;
using VRageMath;

namespace IngameScript.Ship.Components.Missiles.GuidanceObjective
{
    internal class DirectAttackBehavior : IMissionBehavior
    {
        private TrackableShip _target;
        private Missile _missile;

        // Guidance parameters
        private const float MaxThrustOutput = 1.0f;
        private const float ProximityThreshold = 5.0f; // Distance at which missile is considered to have reached target
        private const float PropNavGain = 3.0f; // Proportional navigation gain (N = 3 is typical)
        private const float MaxRotationRate = 2.0f; // Maximum rotation rate command

        // Previous frame data for propnav
        private Vector3D _previousLineOfSight = Vector3D.Zero;
        private bool _isFirstEvaluation = true;

        public DirectAttackBehavior(TrackableShip target)
        {
            _target = target;
        }
        
        private AT_Vector3D Position => _missile.Position;
        private Vector3D Forward => _missile.Forward;
        private Vector3D Up => _missile.Up;

        public GuidanceCommand Evaluate()
        {
            if (_target == null || _target.Defunct)
            {
                return new GuidanceCommand { Thrust = 0, Rotation = Vector3D.Zero };
            }

            // Calculate line-of-sight vector from missile to target
            Vector3D lineOfSight = (Vector3D)(_target.Position - Position);
            double distanceToTarget = lineOfSight.Length();

            // Normalize line-of-sight vector
            Vector3D losDirection = Vector3D.Zero;
            if (distanceToTarget > 0.001)
            {
                losDirection = lineOfSight / distanceToTarget;
            }

            // Calculate line-of-sight rate (change in LOS direction)
            Vector3D losRate = Vector3D.Zero;
            if (!_isFirstEvaluation && _previousLineOfSight.LengthSquared() > 0.001)
            {
                losRate = losDirection - _previousLineOfSight;
            }
            _previousLineOfSight = losDirection;
            _isFirstEvaluation = false;

            // Proportional Navigation: rotation command proportional to LOS rate
            // Commanded rotation = N * (missile velocity) × (LOS rate)
            // Simplified: rotation perpendicular to forward, proportional to LOS rate
            Vector3D rotationCommand = Vector3D.Zero;

            if (losRate.LengthSquared() > 0.001)
            {
                // Cross product of forward direction with LOS rate gives rotation axis
                rotationCommand = Vector3D.Cross(Forward, losRate) * PropNavGain;

                // Clamp rotation rate to maximum
                double rotationMagnitude = rotationCommand.Length();
                if (rotationMagnitude > MaxRotationRate)
                {
                    rotationCommand = (rotationCommand / rotationMagnitude) * MaxRotationRate;
                }
            }

            // Full thrust toward target
            float thrustCommand = MaxThrustOutput;

            return new GuidanceCommand
            {
                Rotation = rotationCommand,
                Thrust = thrustCommand
            };
        }

        public bool IsComplete()
        {
            if (_target == null || _target.Defunct)
            {
                return true;
            }

            // Check if missile is within proximity threshold of target
            double distanceToTarget = (_target.Position - Position).Length();
            return distanceToTarget < ProximityThreshold;
        }

        public void OnComplete()
        {
            // Optional cleanup - could be used for detonation logic or other end-of-mission tasks
            // Currently no specific cleanup needed
        }

        public void BindToMissile(Missile missile)
        {
            _missile = missile;
        }
    }


}