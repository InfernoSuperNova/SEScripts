using System;
using System.Collections.Generic;
using EmptyKeys.UserInterface.Controls.Primitives;
using IngameScript.Helper;
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
        
        private static List<TrackableShip> _reusedShipList = new List<TrackableShip>();
        
        public static Dictionary<long, TargetTracker> EntityIdToTracker = new Dictionary<long, TargetTracker>();
        
        private static IEnumerator<TrackableShip> _pollEnumerator;

        private static MyDynamicAABBTreeD _trackableShipTree = new MyDynamicAABBTreeD();

        static ShipManager()
        {
            
        }
        public static ControllableShip PrimaryShip { get; set; }

        
        
        public static void EarlyUpdate(int frame)
        {
            UpdatePollFrequency();
            for (var index = AllShips.Count - 1; index >= 0; index--)
            {
                var ship = AllShips[index];

                ship.EarlyUpdate(frame);
            }



            foreach (var ship in AllShips)
            {
                var trackable = ship as TrackableShip;
                if (trackable == null) continue;
                var box = trackable.WorldAABB;
                    
                if (trackable.ProxyId != 0)
                {
                    var displacement = trackable.TakeDisplacement();
                    _trackableShipTree.MoveProxy(trackable.ProxyId, ref box, displacement);
                }
                else
                {
                    trackable.ProxyId = _trackableShipTree.AddProxy(ref box, trackable, 0U);
                }

            }
            

            foreach (var kvp in DynamicAABBTreeDHelper.GetAllOverlaps(_trackableShipTree))
            {
                var testShip = kvp.Key;
                var overlaps = kvp.Value;

                var shipSize = testShip.WorldAABB.Size.LengthSquared();

                foreach (var overlapShip in overlaps)
                {
                    

                    var overlapShipSize = overlapShip.WorldAABB.Size.LengthSquared();

                    // var outcome = overlapShipSize > shipSize ? "losing" : "winning";
                    // Program.Log($"Ship {testShip?.Name} is overlapping ship {overlapShip?.Name} and {outcome}");
                    
                    if (overlapShipSize > shipSize)
                    {
                        testShip.IntersectsLargerShipAABB = true;
                        return;
                    }
                }

                testShip.IntersectsLargerShipAABB = false;
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
                if (index >= AllShips.Count) continue;
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
            var trackerGroup = gridTerminalSystem.GetBlockGroupWithName(Config.TrackerGroupName);
            if (trackerGroup == null)
            {
                throw new Exception(
                    $"Tracker group missing! Please create a group called {Config.TrackerGroupName} containing custom turret controllers, a rotor and a fixed gun.");
            }
            var blocks = new List<IMyTerminalBlock>();
            var trackerBlocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            trackerGroup.GetBlocks(trackerBlocks);
            var ship = new ControllableShip(grid, blocks, trackerBlocks);
            AllShips.Add(ship);
            PrimaryShip = ship;

            
            _pollEnumerator = UpdatePollFrequenciesOneByOne().GetEnumerator();
        }

        public static TrackableShip AddTrackableShip(TargetTracker tracker, long entityId, MyDetectedEntityInfo initial)
        {
            var trackableShip = new TrackableShip(tracker, entityId, initial);
            AllShips.Add(trackableShip);
            EntityIdToTracker.Add(entityId, tracker);
            return trackableShip;
        }
        public static void RemoveTrackableShip(TrackableShip trackableShip)
        {
            AllShips.Remove(trackableShip);
            EntityIdToTracker.Remove(trackableShip.EntityId);
            _trackableShipTree.RemoveProxy(trackableShip.ProxyId);
        }


    }
}