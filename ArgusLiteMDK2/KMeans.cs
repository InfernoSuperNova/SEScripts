using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    public class KMeans
    {
        public static MyGridProgram program;
        private readonly List<Vector3D> centroids;

        private readonly List<Vector3D> dataPoints;
        private readonly int k;
        private readonly int maxIterations;

        public KMeans(List<Vector3D> dataPoints, int k, int maxIterations)
        {
            this.dataPoints = dataPoints;
            this.k = k;
            this.maxIterations = maxIterations;
            centroids = new List<Vector3D>();
        }

        public double ComputeWCSS(List<Vector3D>[] clusters)
        {
            double wcss = 0;
            for (var i = 0; i < k; i++)
                foreach (var point in clusters[i])
                    wcss += Math.Pow(Vector3D.Distance(point, centroids[i]), 2);
            return wcss;
        }

        public List<Vector3D>[] Cluster()
        {
            InitializeCentroids();
            var clusters = new List<Vector3D>[k];
            for (var i = 0; i < k; i++) clusters[i] = new List<Vector3D>();

            for (var iter = 0; iter < maxIterations; iter++)
            {
                // Assign each hudSurfaceDataList point to the nearest centroid
                for (var i = 0; i < dataPoints.Count; i++)
                {
                    var nearestCentroidIndex = FindNearestCentroidIndex(dataPoints[i]);
                    clusters[nearestCentroidIndex].Add(dataPoints[i]);
                }

                // Update centroids
                for (var i = 0; i < k; i++)
                {
                    if (clusters[i].Count == 0)
                        continue;
                    centroids[i] = ComputeCentroid(clusters[i]);
                }

                // Clear clusters
                foreach (var cluster in clusters) cluster.Clear();
            }

            return clusters;
        }

        private void InitializeCentroids()
        {
            var rand = new Random();
            var chosenIndices = new HashSet<int>();

            while (centroids.Count < k)
            {
                var index = rand.Next(dataPoints.Count);

                if (!chosenIndices.Contains(index))
                {
                    chosenIndices.Add(index);
                    centroids.Add(dataPoints[index]);
                }
            }
        }

        private int FindNearestCentroidIndex(Vector3D point)
        {
            var nearestIndex = 0;
            var minDistance = double.MaxValue;

            for (var i = 0; i < k; i++)
            {
                var distance = Vector3D.Distance(point, centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private Vector3D ComputeCentroid(List<Vector3D> cluster)
        {
            if (cluster.Count == 0)
                return Vector3D.Zero;

            var sum = Vector3D.Zero;
            foreach (var point in cluster) sum += point;

            return sum / cluster.Count;
        }
    }
}