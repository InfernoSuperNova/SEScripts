using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class ArgusTargetableShip : ArgusShip
    {
        public static readonly ArgusTargetableShip Default = new ArgusTargetableShip(0, MatrixD.Identity, Vector3D.Zero, 0);
        
        private bool _calculationRequired = true;
        
        public ulong ScanFrame;
        
        
    #region Default
        public readonly List<TargetableBlock> Blocks;
        public int AllBlocksCount;
        private Vector3D _allBlocksTargetPosition;
    #endregion
        
    #region Weapon
        public readonly List<TargetableBlock> WeaponBlocks;
        public int WeaponBlocksCount;
        private Vector3D _weaponTargetPosition;
    #endregion
        
    #region Propulsion
        public readonly List<TargetableBlock> PropulsionBlocks;
        public int PropulsionBlocksCount;
        private Vector3D _propulsionTargetPosition;
    #endregion
        
    #region Power Systems
        public readonly List<TargetableBlock> PowerSystemsBlocks;
        public int PowerSystemsBlocksCount;
        private Vector3D _powerSystemsTargetPosition;
    #endregion
        
        // TODO: add ability to aim at uncategorized
    #region Uncategorized
        public readonly List<TargetableBlock> UncategorizedBlocks;
        public int UncategorizedBlocksCount;
        private Vector3D _uncategorizedTargetPosition;
    #endregion
        
        public ArgusTargetableShip(long entityId, MatrixD toWorldCoordinatesMatrix, Vector3D position, ulong scanFrame) : base(entityId, toWorldCoordinatesMatrix, position, null)
        {
            Blocks = new List<TargetableBlock>();
            WeaponBlocks = new List<TargetableBlock>();
            PropulsionBlocks = new List<TargetableBlock>();
            PowerSystemsBlocks = new List<TargetableBlock>();
            UncategorizedBlocks = new List<TargetableBlock>();

            _allBlocksTargetPosition = Vector3D.Zero;
            _weaponTargetPosition = Vector3D.Zero;
            _propulsionTargetPosition = Vector3D.Zero;
            _powerSystemsTargetPosition = Vector3D.Zero;
            this.ScanFrame = scanFrame;
        }


        public void CalculateTargetPositions()
        {
            if (!_calculationRequired) return;
            _calculationRequired = false;
            AllBlocksCount = 0;
            WeaponBlocksCount = 0;
            PropulsionBlocksCount = 0;
            PowerSystemsBlocksCount = 0;
            UncategorizedBlocksCount = 0;

            var allBlocks = new List<Vector3D>();
            var weaponBlocks = new List<Vector3D>();
            var propulsionBlocks = new List<Vector3D>();
            var powerSystemsBlocks = new List<Vector3D>();
            var uncategorizedBlocks = new List<Vector3D>();


            foreach (var block in Blocks)
            {
                var pos = block.positionInGrid * 2.5;
                var cat = block.category;
                switch (cat)
                {
                    case TargetableBlockCategory.Default:
                        uncategorizedBlocks.Add(pos);
                        UncategorizedBlocksCount++;
                        allBlocks.Add(pos);
                        AllBlocksCount++;
                        UncategorizedBlocks.Add(block);
                        break;
                    case TargetableBlockCategory.Weapons:
                        weaponBlocks.Add(pos);
                        WeaponBlocksCount++;
                        allBlocks.Add(pos);
                        AllBlocksCount++;
                        WeaponBlocks.Add(block);
                        break;
                    case TargetableBlockCategory.Propulsion:
                        propulsionBlocks.Add(pos);
                        PropulsionBlocksCount++;
                        allBlocks.Add(pos);
                        AllBlocksCount++;
                        PropulsionBlocks.Add(block);
                        break;
                    case TargetableBlockCategory.PowerSystems:
                        powerSystemsBlocks.Add(pos);
                        PowerSystemsBlocksCount++;
                        allBlocks.Add(pos);
                        AllBlocksCount++;
                        PowerSystemsBlocks.Add(block);
                        break;
                }
            }


            _allBlocksTargetPosition = CalculateClusteredPosition(allBlocks);
            _weaponTargetPosition = CalculateClusteredPosition(weaponBlocks);
            _propulsionTargetPosition = CalculateClusteredPosition(propulsionBlocks);
            _powerSystemsTargetPosition = CalculateClusteredPosition(powerSystemsBlocks);
            _uncategorizedTargetPosition = CalculateClusteredPosition(uncategorizedBlocks);
        }
        private Vector3D CalculateClusteredPosition(List<Vector3D> positions)
        {
            double maxWeight = 0;
            var weights = new Dictionary<Vector3D, double>();
            for (var i = 0; i < positions.Count; i++)
            {
                var pos = positions[i];
                double weight = 0;
                for (var i1 = 0; i1 < positions.Count; i1++)
                {
                    if (i == i1) continue;
                    var posB = positions[i1];
                    weight += (posB - pos).ALengthSquared();
                }

                if (weight > maxWeight) maxWeight = weight;
                weights.Add(pos, weight);
            }

            double totalWeight = 0;
            var averagePos = Vector3D.Zero;
            foreach (var kvp in weights)
            {
                var weight = maxWeight - kvp.Value;
                totalWeight += weight;
                averagePos += kvp.Key * weight;
            }

            if (totalWeight == 0) return averagePos;
            averagePos /= totalWeight;

            return averagePos;
        }

        // to be done later
        //public void CalculateTargetPositions()
        //{

        //    List<Vector3D> weaponBlocks = new List<Vector3D>();
        //    allBlocksCount = 0;
        //    weaponBlocksCount = 0;
        //    propulsionBlocksCount = 0;
        //    powerSystemsBlocksCount = 0;
        //    uncategorizedBlocksCount = 0;
        //    foreach (var block in Blocks)
        //    {
        //        Vector3D pos = block.positionInGrid * 2.5;
        //        TargetableBlockCategory cat = block.category;
        //        switch (cat)
        //        {
        //            case TargetableBlockCategory.Default:
        //                UncategorizedTargetPosition += pos;
        //                uncategorizedBlocksCount++;
        //                AllBlocksTargetPosition += pos;
        //                allBlocksCount++;
        //                break;
        //            case TargetableBlockCategory.Weapons:
        //                //WeaponTargetPosition += pos;
        //                weaponBlocksCount++;
        //                weaponBlocks.Add(pos);
        //                AllBlocksTargetPosition += pos;
        //                allBlocksCount++;
        //                break;
        //            case TargetableBlockCategory.Propulsion:
        //                PropulsionTargetPosition += pos;
        //                propulsionBlocksCount++;
        //                AllBlocksTargetPosition += pos;
        //                allBlocksCount++;
        //                break;
        //            case TargetableBlockCategory.PowerSystems:
        //                PowerSystemsTargetPosition += pos;
        //                powerSystemsBlocksCount++;
        //                AllBlocksTargetPosition += pos;
        //                allBlocksCount++;
        //                break;
        //        }
        //    }

        //    Dictionary<int, KeyValuePair<List<Vector3D>[], double>> kMeansResults = new Dictionary<int, KeyValuePair<List<Vector3D>[], double>>();
        //    for (int k = 1; k < 15; k++)
        //    {
        //        var kMeans = new KMeans(weaponBlocks, k, 5);
        //        var clusters = kMeans.Cluster();
        //        //Program.Echo(clusters.Count().ToString());
        //        //var e = 0 / k; // intentional division by zero
        //        double wcss = kMeans.ComputeWCSS(clusters);
        //        kMeansResults.Add(k, new KeyValuePair<List<Vector3D>[], double>(clusters, wcss));
        //    }

        //    int optimalK = FindElbowPoint(kMeansResults);
        //    Program.Echo("Optimal key: " + optimalK);
        //    List<Vector3D> optimalCluster = kMeansResults[optimalK].Key[0];
        //    //WeaponTargetPosition = optimalCluster.Aggregate((acc, cur) => acc + cur) / optimalCluster.Count;
        //    Program.Echo("All block count: " + allBlocksCount);
        //    Program.Echo("Weapon block count: " + weaponBlocksCount);
        //    Program.Echo("Propulsion block count: " + propulsionBlocksCount);
        //    Program.Echo("Power systems block count: " + powerSystemsBlocksCount);
        //    Program.Echo("Uncategorized block count: " + uncategorizedBlocksCount);
        //    if (allBlocksCount > 0) AllBlocksTargetPosition /= allBlocksCount;
        //    //if (weaponBlocksCount > 0) WeaponTargetPosition /= weaponBlocksCount;
        //    if (propulsionBlocksCount > 0) PropulsionTargetPosition /= propulsionBlocksCount;
        //    if (powerSystemsBlocksCount > 0) PowerSystemsTargetPosition /= powerSystemsBlocksCount;
        //    if (uncategorizedBlocksCount > 0) UncategorizedTargetPosition /= uncategorizedBlocksCount;

        //    // Todo: k-means clustering
        //}
        private static int FindElbowPoint(Dictionary<int, KeyValuePair<List<Vector3D>[], double>> kMeansResults)
        {
            var wcssValues = new double[kMeansResults.Count];
            var i = 0;
            foreach (var kvp in kMeansResults)
            {
                wcssValues[i] = kvp.Value.Value;
                i++;
            }

            // Find the elbow point
            var optimalK = 0;
            double maxChange = 0;
            for (i = 1; i < wcssValues.Length; i++)
            {
                var change = Math.Abs(wcssValues[i] - wcssValues[i - 1]);
                if (change > maxChange)
                {
                    maxChange = change;
                    optimalK = i + 1; // K starts from 1, so increment by 1
                }
            }

            return optimalK;
        }

        public Vector3D GetTargetPosition(TargetableBlockCategory cat)
        {
            switch (cat)
            {
                case TargetableBlockCategory.Default:
                    return Vector3D.Transform(_allBlocksTargetPosition, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.Weapons:
                    return Vector3D.Transform(_weaponTargetPosition, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.Propulsion:
                    return Vector3D.Transform(_propulsionTargetPosition, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.PowerSystems:
                    return Vector3D.Transform(_powerSystemsTargetPosition, ToWorldCoordinatesMatrix);
                default:
                    return Vector3D.Zero;
            }
        }

        public bool UpdateBlockCategoryAtPosition(Vector3I positionInGrid, TargetableBlockCategory cat)
        {
            for (var i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                if (block.positionInGrid == positionInGrid)
                {
                    if (block.category == TargetableBlockCategory.Default) block.category = cat;

                    return true;
                }
            }

            return false;
        }


        public static MatrixD GetToWorldCoordinatesMatrix(MyDetectedEntityInfo targetInfo)
        {
            var toWorldMatrix = targetInfo.Orientation;
            var targetShipPosition = targetInfo.Position;
            toWorldMatrix.Translation = targetShipPosition;


            return toWorldMatrix;
        }

        public Vector3D GetBlockPositionAtIndex(int index)
        {
            if (index == -1) return Position;
            return Vector3D.Transform(2.5 * Blocks[index].positionInGrid, ToWorldCoordinatesMatrix);
        }

        public Vector3D GetBlockPositionOfCategoryAtIndex(TargetableBlockCategory cat, int index)
        {
            if (index == -1) return Position;
            switch (cat)
            {
                case TargetableBlockCategory.Default:
                    if (index >= Blocks.Count) return Vector3D.Zero;
                    return Vector3D.Transform(2.5 * Blocks[index].positionInGrid, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.Weapons:
                    if (index >= WeaponBlocks.Count) return Vector3D.Zero;
                    return Vector3D.Transform(2.5 * WeaponBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.Propulsion:
                    if (index >= PropulsionBlocks.Count) return Vector3D.Zero;
                    return Vector3D.Transform(2.5 * PropulsionBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
                case TargetableBlockCategory.PowerSystems:
                    if (index >= PowerSystemsBlocks.Count) return Vector3D.Zero;
                    return Vector3D.Transform(2.5 * PowerSystemsBlocks[index].positionInGrid, ToWorldCoordinatesMatrix);
                default:
                    return Vector3D.Zero;
            }
        }

        public int GetBlocksCountOfCategory(TargetableBlockCategory cat)
        {
            switch (cat)
            {
                case TargetableBlockCategory.Default:
                    return AllBlocksCount;
                case TargetableBlockCategory.Weapons:
                    return WeaponBlocksCount;
                case TargetableBlockCategory.Propulsion:
                    return PropulsionBlocksCount;
                case TargetableBlockCategory.PowerSystems:
                    return PowerSystemsBlocksCount;
                default:
                    return 0;
            }
        }
    }


}