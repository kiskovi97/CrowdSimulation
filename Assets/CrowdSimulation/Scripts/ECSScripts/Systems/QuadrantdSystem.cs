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
}

public class QuadrantSystem : ComponentSystem
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantHashMap;
    private static readonly int quadrandCellSize = 10;
    private static readonly int quadrandMultiplyer = 100000;

    public static int GetPositionHashMapKey(float3 position)
    {
        //return 1;
        return (int)(math.floor(position.x / quadrandCellSize) + (quadrandMultiplyer * math.floor(position.z / quadrandCellSize)));
    }

    public static int GetPositionHashMapKey(float3 position, float3 distance)
    {
        //return 1;
        return GetPositionHashMapKey(position + distance * quadrandCellSize);
    }

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

    [BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, Walker>
    {
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref Walker walker)
        {
            int key = GetPositionHashMapKey(translation.Value);
            quadrantHashMap.Add(key, new QuadrantData()
            {
                position = translation.Value,
                direction = walker.direction,
                broId = walker.broId,
            });
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker));
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
