using Assets.CrowdSimulation.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{

    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(GraphSystem))]
    class DijsktraSystem : ComponentSystem
    {
        public static NativeList<float> shortestPath;
        private static NativeList<int> previousPoint;
        private static NativeList<float3> goalPoints;

        private static int oneLayer;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            if (shortestPath.IsCreated) shortestPath.Dispose();
            if (previousPoint.IsCreated) previousPoint.Dispose();
            if (goalPoints.IsCreated) goalPoints.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            InitilaizeIfNecesearry();
            DrawGraph();
        }

        private void DrawGraph()
        {

            for (int i = 0; i < shortestPath.Length; i++)
            {
                var goalPoint = i / oneLayer;
                if (goalPoint == 0) continue;
                if (shortestPath[i] >= 0)
                {
                    if (previousPoint[i] == -1)
                    {
                        DebugProxy.DrawLine(GraphSystem.graphPoints[i % oneLayer].posOuter, goalPoints[goalPoint], Color.black);

                    }
                    else
                    {
                        var goal = GraphSystem.graphPoints[previousPoint[i]].posOuter;
                        var from = GraphSystem.graphPoints[i % oneLayer].posOuter;
                        DebugProxy.DrawLine(from,  (from+ goal * 4f) /5f, Color.green);
                    }
                }
                else
                {
                    DebugProxy.DrawLine(GraphSystem.graphPoints[i % oneLayer].posOuter, goalPoints[goalPoint], Color.red);
                }
            }
        }

        public static void AddGoalPoint(float3 goalPoint)
        {
            InitilaizeIfNecesearry();
            var pointsAvailable = GraphSystem.PointsAvaialbe(goalPoint);
            GenerateShortestPath(goalPoint, pointsAvailable);
            goalPoints.Add(goalPoint);
        }

        private static void GenerateShortestPath(float3 goal, List<int> goals)
        {
            var oneLayer = GraphSystem.graphPoints.Length;
            if (DijsktraSystem.oneLayer != oneLayer)
            {
                DijsktraSystem.oneLayer = oneLayer;
                Recalculate();
            }
            var na = new NativeArray<float>(oneLayer, Allocator.TempJob);
            var pp = new NativeArray<int>(oneLayer, Allocator.TempJob);

            var offset = shortestPath.Length;

            shortestPath.AddRange(na);
            previousPoint.AddRange(pp);

            var used = new HashSet<int>();
            var unused = new HashSet<int>();

            for (int i = 0; i < oneLayer; i++)
            {
                shortestPath[i + offset] = -1;
                previousPoint[i + offset] = -1;
                unused.Add(i);
                if (goals.Contains(i))
                {
                    shortestPath[i + offset] = math.length(shortestPath[i + offset] - goal);
                }
                else
                {
                    shortestPath[i + offset] = -1;
                }
            }
            var iteration = 0;
            while (unused.Count > 0 && iteration < oneLayer)
            {
                iteration++;
                CalculateIterative(used, unused, offset);
            }
        }

        private static void Recalculate()
        {
            var na = new NativeArray<float>(oneLayer * goalPoints.Length, Allocator.TempJob);
            var pp = new NativeArray<int>(oneLayer * goalPoints.Length, Allocator.TempJob);

            shortestPath.AddRange(na);
            previousPoint.AddRange(pp);


            for (int i = 0; i < oneLayer * goalPoints.Length; i++)
            {
                shortestPath[i] = -1;
                previousPoint[i] = -1;
            }
        }

        private static void InitilaizeIfNecesearry()
        {
            if (!shortestPath.IsCreated)
                shortestPath = new NativeList<float>(Allocator.Persistent);
            if (!previousPoint.IsCreated)
                previousPoint = new NativeList<int>(Allocator.Persistent);
            if (!goalPoints.IsCreated)
                goalPoints = new NativeList<float3>(Allocator.Persistent);
        }

        private static void CalculateIterative(HashSet<int> used, HashSet<int> unused, int offset)
        {
            var min = -1;
            foreach (var index in unused)
            {
                if (shortestPath[index + offset] >= 0)
                {
                    if (min == -1)
                    {
                        min = index;
                    }
                    else
                    {
                        if (shortestPath[min + offset] > shortestPath[index + offset])
                        {
                            min = index;
                        }
                    }
                }
            }

            if (min == -1) return;
            used.Add(min);
            unused.Remove(min);

            var minValue = shortestPath[min + offset];

            for (int i = 0; i < GraphSystem.graphPoints.Length; i++)
            {
                var isRoad = GraphSystem.graph[GraphSystem.Index(min, i)];
                if (isRoad)
                {
                    var length = math.length(GraphSystem.graphPoints[i].posOuter - GraphSystem.graphPoints[min].posOuter);
                    if (shortestPath[i + offset] == -1 || shortestPath[i + offset] > minValue + length)
                    {
                        shortestPath[i + offset] = minValue + length;
                        previousPoint[i + offset] = min % oneLayer;
                    }
                }
            }
        }
    }
}
