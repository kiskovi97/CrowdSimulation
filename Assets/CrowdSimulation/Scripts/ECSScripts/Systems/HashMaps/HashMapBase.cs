using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public class HashMapBase<Tdata> : ComponentSystem where Tdata : struct, IComponentData
    {
        public static NativeMultiHashMap<int, MyData> quadrantHashMap;

        public struct MyData
        {
            public Entity entity;
            public Tdata data;
            public float3 position;
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

        protected override void OnUpdate()
        {
            EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata));
            quadrantHashMap.Clear();
            if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
            {
                quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            Entities.ForEach((Entity entity, ref Translation translation, ref Tdata data) =>
            {
                int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
                quadrantHashMap.Add(key, new MyData()
                {
                    entity = entity,
                    position = translation.Value,
                    data = data
                });
            });
        }
    }

    public class HashMapBase<Tdata, Tdata2> : ComponentSystem
        where Tdata : struct, IComponentData
        where Tdata2 : struct, IComponentData
    {
        public static NativeMultiHashMap<int, MyData> quadrantHashMap;

        public struct MyData
        {
            public Tdata data;
            public Tdata2 data2;
            public float3 position;
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

        protected override void OnUpdate()
        {
            EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata), typeof(Tdata2));
            quadrantHashMap.Clear();
            if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
            {
                quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            Entities.ForEach((ref Translation translation, ref Tdata data, ref Tdata2 data2) =>
            {
                int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
                quadrantHashMap.Add(key, new MyData()
                {
                    position = translation.Value,
                    data = data,
                    data2 = data2,
                });
            });
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

        protected override void OnUpdate()
        {
            EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Tdata), typeof(Tdata2), typeof(Tdata3));
            quadrantHashMap.Clear();
            if (entityQuery.CalculateEntityCount() > quadrantHashMap.Capacity)
            {
                quadrantHashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            Entities.ForEach((ref Translation translation, ref Tdata data, ref Tdata2 data2, ref Tdata3 data3) =>
            {
                int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
                quadrantHashMap.Add(key, new MyData()
                {
                    position = translation.Value,
                    data = data,
                    data2 = data2,
                    data3 = data3,
                });
            });
        }
    }
}
