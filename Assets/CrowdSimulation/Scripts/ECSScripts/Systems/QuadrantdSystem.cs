using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct QuadrantData
{
    public float3 direction;
    public float3 position;
    public int broId;
    public float radius;
}

public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantHashMap;

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> map, int key)
    {
        return map.CountValuesForKey(key);
    }

    protected override void OnCreate()
    {
        quadrantHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantHashMap.Dispose();
        base.OnDestroy();
    }
    
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker), typeof(CollisionParameters));
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
