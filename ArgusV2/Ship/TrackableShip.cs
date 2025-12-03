using System;
using System.Collections.Generic;
using IngameScript.Helper;
using IngameScript.Ship.Components;
using IngameScript.TruncationWrappers;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Library.Collections;
using VRage.Utils;
using VRageMath;

namespace IngameScript.Ship
{


    enum ScannedBlockType
    {
        Any,
        Weapons,
        Propulsion,
        PowerSystems,
        LikelyDecoy
    }
    class ScannedBlockTracker
    {
        private int _timer;
        private readonly int _initialTimer;
        public ScannedBlockType Type;

        public ScannedBlockTracker(int timer, ScannedBlockType type)
        {
            _timer = timer;
            _initialTimer = timer;
            Type = type;
        }
        public void ResetTimer()
        {
            _timer = _initialTimer;
        }

        public bool Countdown()
        {
            _timer--;
            return _timer <= 0;
        }
    }
    
    
    
    /// <summary>
    /// Represents an enemy ship that can be tracked by Argus but cannot be controlled by Argus.
    /// </summary>
    public class TrackableShip : ArgusShip
    {
        
        private Dictionary<Vector3I, ScannedBlockTracker> _scannedBlocks = new Dictionary<Vector3I, ScannedBlockTracker>();
        private Dictionary<Vector3I, ScannedBlockTracker> _scannedBlocks_Swap = new Dictionary<Vector3I, ScannedBlockTracker>();

        public SupportingShip TrackingShip;

        public AT_DetectedEntityInfo Info;

        public long EntityId;
        private bool _aabbNeedsRecalc = true;
        private BoundingBoxD _cachedAABB;
        private AT_Vector3D _cachedGridOffset;
        private AT_Vector3D _previousPosition;

        private readonly float _gridSize = 1f;

        public TrackableShip(TargetTracker tracker, long entityId, AT_DetectedEntityInfo initial)
        {
            Tracker = tracker;
            EntityId = entityId;
            PollFrequency = PollFrequency.Medium;

            
            switch (initial.Type)
            {
                case MyDetectedEntityType.SmallGrid:
                    _gridSize = 0.5f;
                    break;
                case MyDetectedEntityType.LargeGrid:
                    _gridSize = 2.5f;
                    break;
            }

            Info = initial;
            _worldMatrix = Info.Orientation;
            _worldMatrix.Translation = Info.Position;
        }


        public override AT_Vector3D Position => Info.Position;
        public override AT_Vector3D Velocity => CVelocity;
        public override AT_Vector3D Acceleration => (CVelocity - CPreviousVelocity) * 60;
        public override float GridSize => _gridSize;

        public override string Name => $"Trackable ship {EntityId}";

        public bool Defunct { get; set; } = false;

        public override string ToString() => Name;

        public TargetTracker Tracker { get; set; }
        public bool IntersectsLargerShipAABB { get; set; }

        public BoundingBoxD WorldAABB => Info.BoundingBox;
        
        public AT_Vector3D Extents => LocalAABB.Extents;
        public AT_Vector3D HalfExtents => LocalAABB.HalfExtents;
        public int ProxyId { get; set; } = 0;
        

        public BoundingBoxD LocalAABB
        {
            get
            {
                if (_aabbNeedsRecalc) GenerateLocalAABB();
                return _cachedAABB;
            }
        }

        public AT_Vector3D GridOffset
        {
            get
            {
                if (_aabbNeedsRecalc) GenerateLocalAABB();
                return _cachedGridOffset;
            }
        }



        private AT_Vector3D _displacement;
        private MatrixD _worldMatrix;

        public AT_Vector3D TakeDisplacement()
        {
            var temp = _displacement;
            _displacement = AT_Vector3D.Zero;
            return temp;
        }

        private void GenerateLocalAABB()
        {
            _cachedAABB = OBBReconstructor.GetMaxInscribedOBB(WorldAABB, Info.Orientation, _gridSize);
            _aabbNeedsRecalc = false;
            var extents = HalfExtents;
            var h = _gridSize / 2;
            _cachedGridOffset = new AT_Vector3D(h - extents.X % _gridSize, h - extents.Y % _gridSize, h - extents.Z % _gridSize);
        }
        public override void EarlyUpdate(int frame)
        {
            if (Defunct) return;
            if ((frame + RandomUpdateJitter) % Polling.GetFramesBetweenPolls(PollFrequency) != 0) return;
            Info = Tracker.GetTargetedEntity();
            if (Tracker.Closed || Info.EntityId != EntityId) Defunct = true;
            CPreviousVelocity = CVelocity;
            CVelocity = (Vector3D)Info.Velocity;
            _displacement = Position - _previousPosition;

            if (PollFrequency == PollFrequency.Realtime && (_displacement * 60 - CVelocity).LengthSquared() > 10000)
            {
                _aabbNeedsRecalc = true;
                Vector3I displacement =
                    (Vector3I)(AT_Vector3D.Transform(_displacement - (CVelocity / 60), MatrixD.Invert(_worldMatrix)) * 2);
                DisplaceTrackedBlocks(displacement);
            }
            
            _worldMatrix = Info.Orientation;
            _worldMatrix.Translation += Info.Position;
            _previousPosition = Position;
        }

        public override void LateUpdate(int frame)
        {
            
            _scannedBlocks_Swap.Clear();
            foreach (var kv in _scannedBlocks)
            {
                var worldPos = AT_Vector3D.Transform(
                    (AT_Vector3D)kv.Key * _gridSize + GridOffset,
                    _worldMatrix
                );

                if (kv.Value.Countdown()) continue;
                _scannedBlocks_Swap[kv.Key] = kv.Value; // keep the entry if still valid
                
                Program.Debug.DrawPoint(worldPos, Color.White, 0.2f, 0.016f, true);
            }

            var temp = _scannedBlocks;
            _scannedBlocks = _scannedBlocks_Swap;
            _scannedBlocks_Swap = temp;
            
            var tempobb = new MyOrientedBoundingBoxD(LocalAABB, Info.Orientation);
            tempobb.Center = WorldAABB.Center;
            
        }

        public void AddTrackedBlock(AT_DetectedEntityInfo info, IMyLargeTurretBase turret = null,
            TargetTracker controller = null)
        {
            if (info.EntityId != Info.EntityId || info.HitPosition == null) return;

            var targetBlockPosition = (AT_Vector3D)info.HitPosition;
            //Program.Debug.DrawPoint(targetBlockPosition, Color.Red, 0.2f, 5f, true);
            var worldDirection = targetBlockPosition - Position;
            var positionInGridDouble =
                AT_Vector3D.TransformNormal(worldDirection,
                    MatrixD.Transpose(
                        _worldMatrix));
            positionInGridDouble-= GridOffset;
            var positionInGridInt = new Vector3I(
                (int)Math.Round(positionInGridDouble.X / _gridSize),
                (int)Math.Round(positionInGridDouble.Y / _gridSize),
                (int)Math.Round(positionInGridDouble.Z / _gridSize)
            );

            var type = ScannedBlockType.Any;
            var targetingGroup = turret != null ? turret.GetTargetingGroup() :
                controller != null ? controller.Block.GetTargetingGroup() : "";
            switch (targetingGroup)
            {
                case "Weapons":
                    type = ScannedBlockType.Weapons;
                    break;
                case "Propulsion":
                    type = ScannedBlockType.Propulsion;
                    break;
                case "Power Systems":
                    type = ScannedBlockType.PowerSystems;
                    break;
            }
            if (_scannedBlocks.ContainsKey(positionInGridInt))
            {
                var existing = _scannedBlocks[positionInGridInt];
                if (type != existing.Type) existing.Type = ScannedBlockType.LikelyDecoy;
                existing.ResetTimer();
                return;
            }
            else
            {
                _scannedBlocks.Add(positionInGridInt, new ScannedBlockTracker(Config.Tracker.ScannedBlockMaxValidFrames, type));
            }
            
        }

        private void DisplaceTrackedBlocks(Vector3I displacement)
        {
            var newBlocks = new Dictionary<Vector3I, ScannedBlockTracker>();

            foreach (var block in _scannedBlocks)
            {
                newBlocks.Add(block.Key + displacement, block.Value);
            }

            _scannedBlocks = newBlocks;
        }
    }
}