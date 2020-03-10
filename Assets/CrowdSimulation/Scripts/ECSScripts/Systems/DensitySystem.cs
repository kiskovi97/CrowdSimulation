using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
class DensitySystem : ComponentSystem
{
    public static NativeArray<float> densityMatrix;

    public static float3 Right => new float3((1f / Map.density), 0, 0);
    public static float3 Up => new float3(0, 0, (1f / Map.density));

    protected override void OnCreate()
    {
        densityMatrix = new NativeArray<float>(Map.AllPoints, Allocator.Persistent);
        for (int i = 0; i < Map.AllPoints; i++)
        {
            densityMatrix[i] = 0f;
        }
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        densityMatrix.Dispose();
        base.OnDestroy();
    }

    private static float3 ConvertToLocal(float3 realWorldPosition)
    {
        return (realWorldPosition + new float3(Map.maxWidth, 0, Map.maxHeight)) * Map.density;
    }

    private static float3 ConvertToWorld(float3 position)
    {
        return position * (1f / Map.density) - new float3(Map.maxWidth, 0, Map.maxHeight);
    }

    public struct KeyDistance
    {
        public int key;
        public float distance;
    }

    public struct IndexAndPosition
    {
        public int index;
        public float3 position;
    }

    public static NativeArray<IndexAndPosition> IndexesFromPoisition(float3 position, float radius)
    {
        var width = (int)(radius * Map.density);
        var array = new NativeArray<IndexAndPosition>(width * width * 4, Allocator.Temp);

        int arrayIndex = 0;

        for (int i = -width; i < width && arrayIndex < array.Length; i++)
        {
            for (int j = -width; j < width && arrayIndex < array.Length; j++)
            {
                var Index = IndexFromPosition(position + Up * i + Right * j, position);
                array[arrayIndex++] = new IndexAndPosition()
                {
                    index = Index.key,
                    position = position + Up * i + Right * j
                };
            }
        }

        return array;
    }

    public static KeyDistance IndexFromPosition(float3 realWorldPosition, float3 prev)
    {
        var indexPosition = ConvertToLocal(realWorldPosition);
        var i = (int)math.round(indexPosition.x);
        var j = (int)math.round(indexPosition.z);

        if (i < 0 || j < 0 || i >= Map.widthPoints || j >= Map.heightPoints)
        {
            return new KeyDistance()
            {
                key = -1,
            };
        }
        return new KeyDistance()
        {
            key = Index(i, j),
            distance = math.length(ConvertToLocal(prev) - math.round(indexPosition)),
        };
    }

    private static int Index(int i, int j)
    {
        return (Map.heightPoints * i) + j;
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker), typeof(CollisionParameters));
        for (int i = 0; i < Map.AllPoints; i++)
        {
            densityMatrix[i] = 0f;
        }
        var job = new SetDensityGridJob()
        {
            quadrantHashMap = densityMatrix,
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();
        int group = 3;

        for (int j = 1; j < Map.heightPoints - 1; j++)
            for (int i = 1; i < Map.widthPoints - 1; i++)
            {
                float right = densityMatrix[Map.OneLayer * group + Index(i + 1, j)];
                float left = densityMatrix[Map.OneLayer * group + Index(i - 1, j)];
                float up = densityMatrix[Map.OneLayer * group + Index(i, j + 1)];
                float down = densityMatrix[Map.OneLayer * group + Index(i, j - 1)];

                var point = ConvertToWorld(new float3(i, 0, j));
                if (right < left && right < up && right < down)
                {
                    DebugProxy.DrawLine(point, point + new float3(0.2f, 0, 0), Color.red);
                    continue;
                }
                if (left < up && left < down && left < right)
                {
                    DebugProxy.DrawLine(point, point + new float3(-0.2f, 0, 0), Color.red);
                    continue;
                }
                if (up < down && up < right && up < left)
                {
                    DebugProxy.DrawLine(point, point + new float3(0, 0, 0.2f), Color.red);
                    continue;
                }
                if (down < left && down < up && down < right)
                {
                    DebugProxy.DrawLine(point, point + new float3(0, 0, -0.2f), Color.red);
                    continue;
                }
            }
    }
}

