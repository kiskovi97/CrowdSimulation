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
    class GraphSystem : ComponentSystem
    {
        public static NativeArray<float3> graphPoints;
        public static NativeArray<bool> graph;
        internal static NativeArray<bool> shapeGraph;


        public static void UpdateGraph()
        {
            ReCalculate();
        }

        protected override void OnCreate()
        {
            graphPoints = new NativeArray<float3>(0,Allocator.Persistent);
            graph = new NativeArray<bool>(0, Allocator.Persistent);
            shapeGraph = new NativeArray<bool>(0, Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            if (graphPoints.IsCreated) graphPoints.Dispose();
            if (graph.IsCreated) graph.Dispose();
            if (shapeGraph.IsCreated) shapeGraph.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
           //DrawGraph();
        }

        private void DrawGraph()
        {
            for (int i=0; i<graphPoints.Length; i++)
                for (int j=0; j<graphPoints.Length; j++)
                {
                    if (graph[Index(i,j)])
                    {
                        DebugProxy.DrawLine(graphPoints[i], graphPoints[j], Color.blue);
                    }
                }
        }

        public static int Index(int from, int to)
        {
            return from * graphPoints.Length + to;
        }

        public static int Index(int from, int to, int count)
        {
            return from * count + to;
        }

        static void ReCalculate()
        {
            int count = 0;
            var shapes = PhysicsShapeConverter.graph.GetShapes();
            foreach (var shape in shapes)
            {
                count += shape.Count;
            }
            if (graph.IsCreated) graph.Dispose();
            if (graphPoints.IsCreated) graphPoints.Dispose();
            if (shapeGraph.IsCreated) shapeGraph.Dispose();
            graph = new NativeArray<bool>(count * count, Allocator.Persistent);
            graphPoints = new NativeArray<float3>(count, Allocator.Persistent);
            shapeGraph = new NativeArray<bool>(count * count, Allocator.Persistent);


            var meIndex = 0;
            for (int sI = 0; sI < shapes.Count; sI++)
            {
                var shape = shapes[sI];
                for (int i = 0; i < shape.Count; i++)
                {
                    var left = i - 1;
                    if (left < 0) left = shape.Count - 1;
                    var lPoint = shape[left];
                    var me = shape[i];

                    graphPoints[meIndex + i] = me;

                    if (IsNotCrossing(me, lPoint))
                    {
                        graph[Index(meIndex + left, meIndex + i)] = true;
                        graph[Index(meIndex + i, meIndex + left)] = true;
                    }


                    shapeGraph[Index(meIndex + left, meIndex + i)] = true;
                    shapeGraph[Index(meIndex + i, meIndex + left)] = true;
                }
                meIndex += shape.Count;
            }
            for (int i = 0; i < graphPoints.Length; i++)
            {
                for (int j = 0; j < graphPoints.Length; j++)
                {
                    if (!graph[Index(i, j)])
                    {
                        if (IsNotCrossing(graphPoints[i], graphPoints[j]) && NotInside(graphPoints[i], graphPoints[j], shapes))
                        {
                            graph[Index(i, j)] = true;
                        }
                    }
                }
            }
        }


        public static List<int> PointsAvaialbe(float3 goalPoint)
        {
            var nodeListA = new List<int>();

            for (int i = 0; i < graphPoints.Length; i++)
            {
                if (IsNotCrossing(goalPoint, graphPoints[i]))
                {
                    nodeListA.Add(i);
                }
            }
            return nodeListA;
        }

        private static bool IsNotCrossing(float3 me, float3 other)
        {
            return IsNotCrossing(me, other, graphPoints, shapeGraph);
        }

        public static bool IsNotCrossing(float3 me, float3 point, NativeArray<float3> graphPoints, NativeArray<bool> shapeGraph)
        {
            for (int i = 0; i < graphPoints.Length; i++)
                for (int j = 0; j < graphPoints.Length; j++)
                {
                    if (i == j) continue;
                    if (!shapeGraph[Index(i, j, graphPoints.Length)]) continue;
                    var other1 = graphPoints[i];
                    var other2 = graphPoints[j];
                    if ((other1.Equals(point) && other2.Equals(me)) || (other1.Equals(me) && other2.Equals(point)))
                    {
                        continue;
                    }
                    if (MyMath.AreInLine(me, point, other1, other2))
                    {
                        if (MyMath.AreIntersect(me, point, other1, other2))
                        {
                            return false;
                        }
                        continue;
                    }
                    if (MyMath.DoIntersect(me, point, other1, other2))
                    {
                        return false;
                    }
                }
            return true;
        }

        public static bool NotInside(float3 me, float3 point, List<List<float3>> shapes)
        {
            foreach (var shape in shapes)
            {
                if (MyMath.InnerPoint((point + me) / 2f, shape.ToArray()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
