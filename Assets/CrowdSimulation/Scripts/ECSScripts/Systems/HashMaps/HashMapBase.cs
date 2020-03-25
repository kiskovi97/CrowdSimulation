using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class HashMapBase<Tdata> : ComponentSystem where Tdata : struct, IComponentData
{
    public static NativeMultiHashMap<int, MyData> quadrantHashMap;

    public struct MyData
    {
        public Tdata data;
        public float3 position;
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, MyData> map, int key)
    {
        return map.CountValuesForKey(key);
    }

    protected override void OnCreate()
    {
        quadrantHashMap = new NativeMultiHashMap<int, MyData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    public struct SetHashMapJob : IJobForEach<Translation, Tdata>
    {
        public NativeMultiHashMap<int, MyData>.ParallelWriter quadrantHashMap;

        public void Execute([ReadOnly]ref Translation translation, [ReadOnly]ref Tdata data)
        {
            int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
            quadrantHashMap.Add(key, new MyData()
            {
                position = translation.Value,
                data = data
            });
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata));
        quadrantHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
        {
            quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var job = new SetHashMapJob()
        {
            quadrantHashMap = quadrantHashMap.AsParallelWriter(),
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();
    }
}

public class HashMapBase<Tdata, Tdata2> : ComponentSystem 
    where Tdata: struct, IComponentData 
    where Tdata2 : struct, IComponentData
{
    public static NativeMultiHashMap<int, MyData> quadrantHashMap;

    public struct MyData
    {
        public Tdata data;
        public Tdata2 data2;
        public float3 position;
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, MyData> map, int key)
    {
        return map.CountValuesForKey(key);
    }

    protected override void OnCreate()
    {
        quadrantHashMap = new NativeMultiHashMap<int, MyData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    public struct SetHashMapJob : IJobForEach<Translation, Tdata, Tdata2>
    {
        public NativeMultiHashMap<int, MyData>.ParallelWriter quadrantHashMap;

        public void Execute([ReadOnly]ref Translation translation, [ReadOnly]ref Tdata data, [ReadOnly]ref Tdata2 data2)
        {
            int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
            quadrantHashMap.Add(key, new MyData()
            {
                position = translation.Value,
                data = data,
                data2 = data2,
            });
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata), typeof(Tdata2));
        quadrantHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
        {
            quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var job = new SetHashMapJob()
        {
            quadrantHashMap = quadrantHashMap.AsParallelWriter(),
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();
    }
}

public class HashMapBase<Tdata, Tdata2, Tdata3> : ComponentSystem
    where Tdata : struct, IComponentData
    where Tdata2 : struct, IComponentData
    where Tdata3 : struct, IComponentData
{
    public static NativeMultiHashMap<int, MyData> quadrantHashMap;

    public struct MyData
    {
        public Tdata data;
        public Tdata2 data2;
        public Tdata3 data3;
        public float3 position;
    }

    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, MyData> map, int key)
    {
        return map.CountValuesForKey(key);
    }

    protected override void OnCreate()
    {
        quadrantHashMap = new NativeMultiHashMap<int, MyData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        quadrantHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    public struct SetHashMapJob : IJobForEach<Translation, Tdata, Tdata2, Tdata3>
    {
        public NativeMultiHashMap<int, MyData>.ParallelWriter quadrantHashMap;

        public void Execute([ReadOnly]ref Translation translation, [ReadOnly]ref Tdata data, [ReadOnly]ref Tdata2 data2, [ReadOnly]ref Tdata3 data3)
        {
            int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
            quadrantHashMap.Add(key, new MyData()
            {
                position = translation.Value,
                data = data,
                data2 = data2,
                data3 = data3,
            });
        }
    }

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata), typeof(Tdata2), typeof(Tdata3));
        quadrantHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
        {
            quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
        }

        var job = new SetHashMapJob()
        {
            quadrantHashMap = quadrantHashMap.AsParallelWriter(),
        };

        var handle = JobForEachExtensions.Schedule(job, entityQuery);
        handle.Complete();
    }
}
