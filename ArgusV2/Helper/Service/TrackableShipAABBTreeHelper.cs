using System;
using System.Collections.Generic;
using IngameScript.Ship;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript.Helper
{
    /// <summary>
    /// Item class.
    /// </summary>
    /// <summary>
    /// Item class.
    /// </summary>
    public static class TrackableShipAABBTreeHelper
    {
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static readonly List<TrackableShip> Elements = new List<TrackableShip>();
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static readonly List<BoundingBoxD> Boxes = new List<BoundingBoxD>();

        private static readonly Dictionary<TrackableShip, List<TrackableShip>> ReusedOverlaps =
            new Dictionary<TrackableShip, List<TrackableShip>>();

        private static readonly List<List<TrackableShip>> ReusedLists = new List<List<TrackableShip>>();

        private static int _rI = 0;
        // Diabolical
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// List method.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        private static List<TrackableShip> GetList()
        {
            if (_rI >= ReusedLists.Count)
                ReusedLists.Add(new List<TrackableShip>(8));

            var list = ReusedLists[_rI++];
            list.Clear();
            return list;
        }
        
        /// <summary>
        /// Returns a static list with references to game objects. CLEAN UP WHEN YOU ARE DONE OR MEMORY WILL LEAK!
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <param name="tree">The tree parameter.</param>
        /// <returns>The result of the operation.</returns>
        /// <summary>
        /// Dictionary method.
        /// </summary>
        /// <param name="tree">The tree parameter.</param>
        /// <returns>The result of the operation.</returns>
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
                overlapping.Clear(); // Clear before returning this list to the cache to avoid memory leaks
            }
            Elements.Clear();
            Boxes.Clear();
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