using System;
using System.Collections.Generic;
using EmptyKeys.UserInterface.Controls.Primitives;
using IngameScript.Ship;
using IngameScript.Ship.Components;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public static class ShipManager
    {
        public static readonly List<ArgusShip> AllShips = new List<ArgusShip>();
        public static readonly Dictionary<long, TrackableShip> EntityIdToTrackableShip = new Dictionary<long, TrackableShip>();
        public static readonly Dictionary<IMyTurretControlBlock, long> TrackerToEntityId = new Dictionary<IMyTurretControlBlock, long>();
        
        private static List<TrackableShip> _reusedShipList = new List<TrackableShip>();
        
        private static IEnumerator<TrackableShip> _pollEnumerator;

        static ShipManager()
        {
            
        }
        public static ControllableShip PrimaryShip { get; set; }

        
        
        public static void PollSensors(int frame)
        {
            UpdatePollFrequency();

            for (var index = AllShips.Count - 1; index >= 0; index--)
            {
                var ship = AllShips[index];

                ship.PollSensors(frame);
                
            }
        }

        private static void UpdatePollFrequency()
        {
            if (!_pollEnumerator.MoveNext())
            {
                // reached the end, restart
                _pollEnumerator = UpdatePollFrequenciesOneByOne().GetEnumerator();
                _pollEnumerator.MoveNext();
            }
        }

        public static void LateUpdate(int frame)
        {
            for (var index = AllShips.Count - 1; index >= 0; index--)
            {
                var ship = AllShips[index];
                ship.LateUpdate(frame);
            }
        }
        
        
        private static IEnumerable<TrackableShip> UpdatePollFrequenciesOneByOne()
        {
            var maxSqr = Config.MaxWeaponRange * Config.MaxWeaponRange;

            for (var index = AllShips.Count - 1; index >= 0; index--)
            {
                var ship = AllShips[index];
                var trackableShip = ship as TrackableShip;
                if (trackableShip == null) continue;

                var pos = trackableShip.Position;
                var ourPos = PrimaryShip.Position;
                var distSqr = (pos - ourPos).LengthSquared();

                if (distSqr > maxSqr)
                    trackableShip.PollFrequency = SensorPollFrequency.Medium;
                else
                    trackableShip.PollFrequency = SensorPollFrequency.Realtime;

                yield return trackableShip; // yields one ship per call
            }
        }
        
        
        
        /// <summary>
        /// Gets trackable ships within a set range of this ship. Do not store the returned list, it WILL be mutated unexpectedly.
        /// </summary>
        /// <param name="ship">The target ship to test.</param>
        /// <param name="range">The search range.</param>
        /// <returns>A REUSED list containing all ships in range. This is REUSED internally and should not be persisted.</returns>
        public static List<TrackableShip> GetTargetsInRange(SupportingShip ship, double range)
        {
            // Just an O(n) lookup for now :)
            _reusedShipList.Clear();
            foreach (var otherShip in AllShips)
            {
                var enemyShip = otherShip as TrackableShip;
                if (enemyShip == null) continue;
                if ((enemyShip.Position - ship.Position).LengthSquared() < range * range) _reusedShipList.Add(enemyShip);
            }
            
            return _reusedShipList;
        }

        public static TrackableShip GetForwardTarget(ControllableShip ship, double range, float coneAngleDegrees)
        {
            var candidates = GetTargetsInRange(ship, range);
            if (candidates.Count < 1) return null;
            
            
            var forward = ship.Forward;
            double cosCone = Math.Cos(coneAngleDegrees * Math.PI / 180.0);
            
            double bestScore = double.MaxValue;
            TrackableShip bestCandidate = null;
            
            // This more or less gets the closest ship closest to the center of the crosshair
            foreach (var candidate in candidates)
            {
                var shipToCandidate = (candidate.Position - ship.Position);
                var length = shipToCandidate.Length();

                var dot = (shipToCandidate / length).Dot(forward);
                dot = MathHelperD.Clamp(dot, -1.0, 1.0);
                if (dot < cosCone) continue;
                
                var score = (1 - dot) * length;
                if (score < bestScore)
                {
                    bestCandidate = candidate;
                    bestScore = score;
                }
            }
            return bestCandidate;
        }

        public static void CreateControllableShip(IMyCubeGrid grid, IMyGridTerminalSystem gridTerminalSystem)
        {
            var group = gridTerminalSystem.GetBlockGroupWithName(Config.GroupName);
            if (group == null)
            {
                throw new Exception($"Group missing! Please create a group called {Config.GroupName}");
            }
            var blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            var ship = new ControllableShip(grid, blocks);
            AllShips.Add(ship);
            PrimaryShip = ship;
            _pollEnumerator = UpdatePollFrequenciesOneByOne().GetEnumerator();
        }

        public static void AddTrackableShip(IMyTurretControlBlock tracker, long entityId)
        {
            var trackableShip = new TrackableShip(tracker, entityId);
            AllShips.Add(trackableShip);
            EntityIdToTrackableShip.Add(entityId, trackableShip);
            TrackerToEntityId.Add(tracker, entityId);
        }
        public static void RemoveTrackableShip(TrackableShip trackableShip)
        {
            AllShips.Remove(trackableShip);
            EntityIdToTrackableShip.Remove(trackableShip.EntityId);
            TrackerToEntityId.Remove(trackableShip.Tracker);
        }


    }
}