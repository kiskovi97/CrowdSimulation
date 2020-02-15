﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct QudrantData
{
    public People people;
    public float3 position;
    public quaternion rotation;
}

public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QudrantData> quadrantHashMap;
    private static readonly int quadrandCellSize = 50;
    private static readonly int quadrandMultiplyer = 100000;

    public static int GetPositionHashMapKey(float3 position)
    {
        return 1;
       // return (int)(math.floor(position.x / quadrandCellSize) + (quadrandMultiplyer * math.floor(position.z / quadrandCellSize)));
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QudrantData> map, int key)
    {
        int count = 0;
        if (map.TryGetFirstValue(key, out QudrantData entity, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                count++;
            } while (map.TryGetNextValue(out entity, ref iterator));
        }
        return count;
    }

    protected override void OnCreate()
    {
        quadrantHashMap = new NativeMultiHashMap<int, QudrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, Rotation, People>
    {
        public NativeMultiHashMap<int, QudrantData>.ParallelWriter quadrantHashMap;


        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, ref People people)
        {
            int key = GetPositionHashMapKey(translation.Value);
            quadrantHashMap.Add(key, new QudrantData()
            {
                people = people,
                position = translation.Value,
                rotation = rotation.Value,
            });
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Rotation), typeof(People));
        quadrantHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
        {
            quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var job = new SetQuadrantDataHashMapJob()
        {
            quadrantHashMap = quadrantHashMap.AsParallelWriter(),
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();
    }
}
