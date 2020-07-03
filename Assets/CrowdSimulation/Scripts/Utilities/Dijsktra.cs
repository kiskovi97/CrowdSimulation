using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.Utilities
{
    class Dijsktra : IDisposable
    {
        public NativeArray<float3> positions;
        public NativeArray<bool> graph;

        private float3 goal;

        private NativeArray<float> shortestPath;
        private NativeArray<int> previousPoint;

        public Dijsktra(NativeArray<float3> positions, NativeArray<bool> graph)
        {
            if (this.positions != null && this.positions.IsCreated) this.positions.Dispose();
            if (this.graph != null && this.graph.IsCreated) this.graph.Dispose();

            this.positions = positions;
            this.graph = graph;

            shortestPath = new NativeArray<float>(positions.Length, Allocator.Persistent);
            previousPoint = new NativeArray<int>(positions.Length, Allocator.Persistent);
        }

        public void CalculatePaths(float3 goal, List<int> goals)
        {
            this.goal = goal;
            var max = positions.Length;

            var used = new HashSet<int>();
            var unused = new HashSet<int>();

            for (int i=0;i< max; i++)
            {
                unused.Add(i);
                if (goals.Contains(i))
                {
                    shortestPath[i] = math.length(positions[i] - goal);
                }
                else
                {
                    shortestPath[i] = -1;
                }
                previousPoint[i] = -1;
            }
            var iteration = 0;
            while (unused.Count > 0 && iteration < max)
            {
                iteration++;
                CalculateIterative(used, unused);
            }
        }

        private void CalculateIterative(HashSet<int> used, HashSet<int> unused)
        {
            var min = -1;
            foreach (var index in unused)
            {
                if (shortestPath[index] >= 0)
                {
                    if (min == -1)
                    {
                        min = index;
                    }
                    else
                    {
                        if (shortestPath[min] > shortestPath[index])
                        {
                            min = index;
                        }
                    }
                }
            }

            if (min == -1) return;
            used.Add(min);
            unused.Remove(min);

            var minValue = shortestPath[min];

            for (int i=0; i<positions.Length; i++)
            {
                var isRoad = graph[NavMeshObject.Index(min, i)];
                if (isRoad)
                {
                    var length = math.length(positions[i] - positions[min]);
                    if (shortestPath[i] == -1 || shortestPath[i] > minValue + length)
                    {
                        shortestPath[i] = minValue + length;
                        previousPoint[i] = min;
                    }
                }
            }
        }

        public List<float3> CalculatePath(float3 pointA, List<int> nodeListA)
        {
            var road = new List<float3>() {
                pointA
            };

            if (nodeListA.Count == 0)
                return road;

            var min = -1;
            foreach (var index in nodeListA)
            {
                if (shortestPath[index] >= 0)
                {
                    if (min == -1)
                    {
                        min = index;
                    }
                    else
                    {
                        if (shortestPath[min] > shortestPath[index])
                        {
                            min = index;
                        }
                    }
                }
            }

            if (min == -1) return road;
            road.Add(positions[min]);

            var iteration = 0;
            while (previousPoint[min] >= 0 && iteration < positions.Length)
            {
                var next = previousPoint[min];
                road.Add(positions[next]);
                min = next;
                iteration++;
            }

            road.Add(goal);

            return road;
        }

        public void Dispose()
        {
            if (this.positions != null && this.positions.IsCreated) this.positions.Dispose();
            if (this.graph != null && this.graph.IsCreated) this.graph.Dispose();
        }
    }
}
