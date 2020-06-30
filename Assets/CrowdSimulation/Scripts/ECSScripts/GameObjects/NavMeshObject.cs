using Assets.CrowdSimulation.Scripts.Utilities;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class NavMeshObject : MonoBehaviour
{
    private NativeArray<float3> positions;
    private NativeArray<bool> graph;
    List<List<float3>> shapes;

    private int count;

    int Index(int from, int to)
    {
        return from * count + to;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhysicsShapeConverter.Changed)
        {
            PhysicsShapeConverter.Changed = false;
            //PhysicsShapeConverter.graph.Draw();
            ReCalculate();
        }
    }

    void ReCalculate()
    {
        count = 0;
        shapes = PhysicsShapeConverter.graph.GetShapes();
        foreach (var shape in shapes)
        {
            count += shape.Count;
        }
        if (graph.IsCreated) graph.Dispose();
        if (positions.IsCreated) positions.Dispose();
        graph = new NativeArray<bool>(count * count, Allocator.Persistent);
        positions = new NativeArray<float3>(count, Allocator.Persistent);
        Debug.Log(count + " : " + positions.Length);
        var meIndex = 0;
        for (int sI = 0; sI < shapes.Count; sI++)
        {
            var shape = shapes[sI];
            for (int i = 0; i < shape.Count; i++)
            {
                var left = i - 1;
                if (left < 0) left = shape.Count - 1;
                var lPoint = shape[left];
                //var right = i + 1;
                //if (right > shape.Count - 1) right = 0;
                //var rPoint = shape[right];
                var me = shape[i];

                positions[meIndex + i] = me;

                if (IsNotCrossing(me, lPoint))
                {
                    AddGraphEdge(meIndex + left, meIndex + i);
                }
                else
                {
                    Debug.DrawLine(me, lPoint, Color.black, 100f);
                }

                var otherIndex = 0;
                for (int sJ = 0; sJ < shapes.Count; sJ++)
                {
                    if (sI == sJ)
                    {
                        otherIndex += shape.Count;
                        continue;
                    }
                    var otherShape = shapes[sJ];
                    for (int j = 0; j < otherShape.Count; j++)
                    {
                        var point = otherShape[j];
                        //if (IsNotBetween(lPoint - me, rPoint - me, point - me))
                        if (IsNotCrossing(me, point))
                            AddGraphEdge(meIndex + i, otherIndex + j);
                    }
                    otherIndex += otherShape.Count;
                }
            }
            meIndex += shape.Count;
        }
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;
                if (graph[Index(i, j)])
                {
                    Debug.DrawLine(positions[i], positions[j], Color.green, 100f);
                }
            }
        }
    }

    public bool IsNotCrossing(float3 me, float3 point)
    {
        foreach (var shape in shapes)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                var left = i - 1;
                if (left < 0) left = shape.Count - 1;
                var other1 = shape[left];
                var other2 = shape[i];
                if (other1.Equals(point) || other1.Equals(me) || other2.Equals(point) || other2.Equals(me)) continue;
                if (MyMath.DoIntersect(me, point, other1, other2))
                {
                    return false;
                }
            }

            if (MyMath.InnerPoint((point + me) / 2f, shape.ToArray()))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsNotBetween(float3 left, float3 right, float3 other)
    {
        var angle = Vector3.SignedAngle(left, right, new float3(0, 1, 0));
        if (angle < 0) angle += 180;
        var angle2 = Vector3.SignedAngle(left, other, new float3(0, 1, 0));
        if (angle2 < 0) angle += 180;
        return angle > angle2;
    }

    private void AddGraphEdge(int from, int to)
    {
        graph[Index(from, to)] = true;
        graph[Index(to, from)] = true;
    }

    private void OnDestroy()
    {
        if (graph.IsCreated) graph.Dispose();
        if (positions.IsCreated) positions.Dispose();
    }
}
