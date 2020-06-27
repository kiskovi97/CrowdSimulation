using Assets.CrowdSimulation.Scripts.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class NavMeshObject : MonoBehaviour
{
    private NativeArray<float3> positions;
    private NativeArray<bool> graph;

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
            count = 0;
            foreach (var shape in PhysicsShapeConverter.points)
            {
                count += shape.Count;
            }
            if (graph.IsCreated) graph.Dispose();
            if (positions.IsCreated) positions.Dispose();
            graph = new NativeArray<bool>(count * count, Allocator.Persistent);
            positions = new NativeArray<float3>(count, Allocator.Persistent);
            Debug.Log(count + " : " + positions.Length);
            var meIndex = 0;
            for (int sI = 0; sI < PhysicsShapeConverter.points.Count; sI++)
            {
                var shape = PhysicsShapeConverter.points[sI];
                for (int i = 0; i < shape.Count; i++)
                {
                    var left = i - 1;
                    if (left < 0) left = shape.Count - 1;
                    var right = i + 1;
                    if (right > shape.Count - 1) right = 0;
                    var lPoint = shape[left];
                    var rPoint = shape[right];
                    var me = shape[i];

                    positions[meIndex + i] = me;

                    if (IsNotCrossing(me, lPoint))
                    {
                        AddGraphEdge(meIndex + left, meIndex + i);
                    }

                    //Debug.DrawLine(me, lPoint, Color.green, 100f);

                    var otherIndex = 0;
                    for (int sJ = 0; sJ < PhysicsShapeConverter.points.Count; sJ++)
                    {
                        if (sI == sJ)
                        {
                            otherIndex += shape.Count;
                            continue;
                        }
                        var otherShape = PhysicsShapeConverter.points[sJ];
                        for (int j = 0; j < otherShape.Count; j++)
                        {
                            var point = otherShape[j];
                            if (IsNotBetween(lPoint - me, rPoint - me, point - me))
                                if (IsNotCrossing(me, point))
                                    AddGraphEdge(meIndex + i, otherIndex + j);
                            // Debug.DrawLine(me, point, Color.green, 100f);
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

        //iteration++;
        //if (iteration > count - 1) iteration = 0;
        //{
        //    for (int j = 0; j < count; j++)
        //    {
        //        if (iteration == j) continue;
        //        if (graph[Index(iteration, j)])
        //        {
        //            Debug.DrawLine(positions[iteration], positions[j], Color.green);
        //        }
        //    }
        //}
    }

    int iteration = 0;

    private bool IsNotCrossing(float3 me, float3 point)
    {
        foreach (var shape in PhysicsShapeConverter.points)
        {
            for (int i = 0; i < shape.Count; i++)
            {
                var left = i - 1;
                if (left < 0) left = shape.Count - 1;
                var other1 = shape[left];
                var other2 = shape[i];
                if (other1.Equals(point) || other1.Equals(me) || other2.Equals(point) || other2.Equals(me)) continue;

                if (MyMath.DoIntersect(me, point, other1, other2)) return false;
            }

            if (MyMath.InnerPoint(me, shape.ToArray())) {
                //Debug.DrawLine(me, shape[0], Color.red, 100f);
                return false;
            }
            if (MyMath.InnerPoint(point, shape.ToArray()))
            {
                //Debug.DrawLine(point, shape[0], Color.red, 100f);
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
