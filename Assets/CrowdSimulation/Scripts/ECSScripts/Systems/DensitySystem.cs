using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class DensitySystem : ComponentSystem
{
    public static readonly int Height = 40;
    public static readonly int Width = 40;
    public NativeArray<float> densityMatrix;

    protected override void OnCreate()
    {
        densityMatrix = new NativeArray<float>(Width * Height, Allocator.Persistent);
        for (int i = 0; i < Width * Height; i++)
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
        return realWorldPosition + new float3(Width / 2, 0, Height / 2);
    }

    private static float3 ConverToWorld(float3 position)
    {
        return position - new float3(Width / 2, 0, Height / 2);
    }

    private struct KeyDistance
    {
        public int key;
        public float distance;
    }

    private static KeyDistance IndexFromPosition(float3 realWorldPosition, float3 prev)
    {
        var position = ConvertToLocal(realWorldPosition);

        if (position.x < 0 || position.z < 0 || position.x >= Width || position.z >= Height)
        {
            return new KeyDistance()
            {
                key = -1,
            };
        }
        return new KeyDistance()
        {
            key = Index((int)math.round(position.x), (int)math.round(position.z)),
            distance = math.length(ConvertToLocal(prev) - math.round(position)),
        };
    }

    private static int Index(int i, int j)
    {
        return (Height * i) + j;
    }

    [BurstCompile]
    private struct SetDensityGridJob : IJobForEachWithEntity<Translation, Walker>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float> quadrantHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref Walker walker)
        {
            float3 pos = translation.Value;
            Add(pos, pos);
            Add(pos + new float3(0, 0, 1), pos);
            Add(pos + new float3(0, 0, -1), pos);
            Add(pos + new float3(1, 0, 0), pos);
            Add(pos + new float3(-1, 0, 0), pos);
        }

        private void Add(float3 position, float3 prev)
        {
            var keyDistance = IndexFromPosition(position, prev);
            if (keyDistance.key >= 0)
                quadrantHashMap[keyDistance.key] += math.max(0f, 1f - keyDistance.distance);
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker));
        for (int i = 0; i < Width * Height; i++)
        {
            densityMatrix[i] = 0f;
        }
        var job = new SetDensityGridJob()
        {
            quadrantHashMap = densityMatrix,
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();

        for (int j = 0; j < Height; j++)
            for (int i = 0; i < Width; i++)
            {
                float height = densityMatrix[Index(i, j)];
                var point = ConverToWorld(new float3(i, 0, j));
                DebugProxy.DrawLine(point, point + new float3(0, height + 0.1f, 0), height > 0 ? Color.red : Color.grey);
            }
    }
}

