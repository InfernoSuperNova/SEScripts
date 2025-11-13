using System;
using System.Collections.Generic;
using IngameScript.Ship;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Helper
{
    public static class DynamicAABBTreeDHelper
    {
        private static readonly List<TrackableShip> Elements = new List<TrackableShip>();
        private static readonly List<BoundingBoxD> Boxes = new List<BoundingBoxD>();

        private static readonly Dictionary<TrackableShip, List<TrackableShip>> ReusedOverlaps =
            new Dictionary<TrackableShip, List<TrackableShip>>();

        private static readonly List<List<TrackableShip>> ReusedLists = new List<List<TrackableShip>>();

        private static int _rI = 0;
        // Diabolical
        private static List<TrackableShip> GetList()
        {
            if (_rI >= ReusedLists.Count)
                ReusedLists.Add(new List<TrackableShip>(8));

            var list = ReusedLists[_rI++];
            list.Clear();
            return list;
        }
        
        public static Dictionary<TrackableShip, List<TrackableShip>> GetAllOverlaps(MyDynamicAABBTreeD tree)
        {
            _rI = 0;
            ReusedOverlaps.Clear();
            // Get all entities and their AABBs
            tree.GetAll(Elements, clear: true, boxsList: Boxes);

            for (int i = 0; i < Elements.Count; i++)
            {
                var entityA = Elements[i];
                var boxA = Boxes[i];

                var overlapping = GetList();
                tree.OverlapAllBoundingBox(ref boxA, overlapping, 0U, false);

                foreach (var entityB in overlapping)
                {
                    if (entityA == entityB) continue;
                    
                    if (!ReusedOverlaps.ContainsKey(entityA)) ReusedOverlaps.Add(entityA, GetList());
                    ReusedOverlaps[entityA].Add(entityB);
                    
                    if (!ReusedOverlaps.ContainsKey(entityB)) ReusedOverlaps.Add(entityB, GetList());
                    ReusedOverlaps[entityB].Add(entityA);
                    
                }
            }

            return ReusedOverlaps;
        }
        public static List<TrackableShip> GetOverlapsWithBox(MyDynamicAABBTreeD tree, BoundingBoxD box)
        {
            _rI = 0;
            // Get a temporary list from the pool
            var overlapping = GetList();

            // Query the tree for all leaf nodes overlapping the given box
            tree.OverlapAllBoundingBox(ref box, overlapping, 0U, false);

            return overlapping;
        }
    }
}