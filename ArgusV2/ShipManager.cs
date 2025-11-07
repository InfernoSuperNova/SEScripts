using System;
using System.Collections.Generic;
using EmptyKeys.UserInterface.Controls.Primitives;
using IngameScript.Ship;
using VRageMath;

namespace IngameScript
{
    public static class ShipManager
    {
        public static readonly List<ArgusShip> AllShips = new List<ArgusShip>();
        
        private static List<TrackableShip> _reusedShipList = new List<TrackableShip>();

        public static void EarlyUpdate(int frame)
        {
            foreach (var ship in AllShips)
            {
                ship.EarlyUpdate(frame);
            }
        }

        public static void LateUpdate(int frame)
        {
            foreach (var ship in AllShips)
            {
                ship.LateUpdate(frame);
            }
        }
        
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ship">The target ship to test.</param>
        /// <param name="range">The search range.</param>
        /// <returns>A REUSED list containing all ships in range.</returns>
        public static List<TrackableShip> GetShipsInRange(SupportingShip ship, double range)
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

        public static TrackableShip GetTargetOffForward(ControllableShip ship, double range, float coneAngleDegrees)
        {
            var candidates = GetShipsInRange(ship, range);
            if (candidates.Count < 1) return null;
            
            
            var forward = ship.Forward;
            double cosCone = Math.Cos(coneAngleDegrees * Math.PI / 180.0);
            
            double lowestCandidacy = double.MaxValue;
            TrackableShip lowestCandidate = null;
            
            // This more or less gets the closest ship closest to the center of the crosshair
            foreach (var candidate in candidates)
            {
                var shipToCandidate = (candidate.Position - ship.Position);
                var length = shipToCandidate.Length();

                var dot = (shipToCandidate / length).Dot(forward);
                dot = MathHelperD.Clamp(dot, -1.0, 1.0);
                if (dot < cosCone) continue;
                
                var candidacy = (1 - dot) * length;
                if (candidacy < lowestCandidacy)
                {
                    lowestCandidate = candidate;
                    lowestCandidacy = candidacy;
                }
            }
            return lowestCandidate;
        }
    }
}