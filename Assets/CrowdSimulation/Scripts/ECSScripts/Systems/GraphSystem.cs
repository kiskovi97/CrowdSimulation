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
    public class GraphSystem : ComponentSystem
    {
        public static NativeArray<GraphPoint> graphPoints;
        public static NativeArray<bool> graph;
        internal static NativeArray<bool> shapeGraph;

        public struct GraphPoint
        {
            public GraphPoint(float3 inner, float3 outer)
            {
                posInner = inner;
                posOuter = outer;
            }

            public float3 posInner;
            public float3 posOuter;

            public override bool Equals(object obj)
            {
                if (obj is GraphPoint point)
                {
                    return posInner.Equals(point.posInner);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return -595007949 + EqualityComparer<float3>.Default.GetHashCode(posInner);
            }
        }

        public static void UpdateGraph()
        {
            ReCalculate();
        }

        protected override void OnCreate()
        {
            graphPoints = new NativeArray<GraphPoint>(0,Allocator.Persistent);
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
                        DebugProxy.DrawLine(graphPoints[i].posOuter, graphPoints[j].posOuter, Color.green);
                    }
                }

            for (int i = 0; i < graphPoints.Length; i++)
                for (int j = 0; j < graphPoints.Length; j++)
                {
                    if (shapeGraph[Index(i, j)])
                    {
                        DebugProxy.DrawLine(graphPoints[i].posInner, graphPoints[j].posInner, Color.blue);
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
            graphPoints = new NativeArray<GraphPoint>(count, Allocator.Persistent);
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

                    if (IsNotCrossing(me.posOuter, lPoint.posOuter))
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
                        if (IsNotCrossing(graphPoints[i].posOuter, graphPoints[j].posOuter) && NotInside(graphPoints[i].posOuter, graphPoints[j].posOuter, shapes))
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
                if (IsNotCrossing(goalPoint, graphPoints[i].posOuter))
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

        public static bool IsNotCrossing(float3 me, float3 point, NativeArray<GraphPoint> graphPoints, NativeArray<bool> shapeGraph)
        {
            for (int i = 0; i < graphPoints.Length; i++)
                for (int j = 0; j < graphPoints.Length; j++)
                {
                    if (i == j) continue;
                    if (!shapeGraph[Index(i, j, graphPoints.Length)]) continue;
                    var other1 = graphPoints[i];
                    var other2 = graphPoints[j];
                    if ((other1.posInner.Equals(point) && other2.posInner.Equals(me)) || (other1.posInner.Equals(me) && other2.posInner.Equals(point)))
                    {
                        continue;
                    }
                    if (MyMath.AreInLine(me, point, other1.posInner, other2.posInner))
                    {
                        if (MyMath.AreIntersect(me, point, other1.posInner, other2.posInner))
                        {
                            return false;
                        }
                        continue;
                    }
                    if (MyMath.DoIntersect(me, point, other1.posInner, other2.posInner))
                    {
                        return false;
                    }
                }
            return true;
        }

        public static bool NotInside(float3 me, float3 point, List<List<GraphPoint>> shapes)
        {
            foreach (var shape in shapes)
            {
                if (MyMath.InnerPoint((point + me) / 2f, shape.Select((p) => p.posInner).ToArray()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
