using Unity.Entities;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Collections;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    class GroupSystem : ComponentSystem
    {
        public static NativeArray<float3> avaragePoint;
        public static NativeArray<float> avarageDistances;
        public static NativeArray<float> minDistances;
        public static NativeArray<float> maxDistances;
        public static NativeArray<int> groupSize;

        protected override void OnCreate()
        {
            base.OnCreate();
            avaragePoint = new NativeArray<float3>(Map.FullGroup, Allocator.Persistent);
            avarageDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
            minDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
            maxDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
            groupSize = new NativeArray<int>(Map.FullGroup, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            if (avaragePoint.IsCreated) avaragePoint.Dispose();
            if (avarageDistances.IsCreated) avarageDistances.Dispose();
            if (minDistances.IsCreated) minDistances.Dispose();
            if (maxDistances.IsCreated) maxDistances.Dispose();
            if (groupSize.IsCreated) groupSize.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (avarageDistances.Length < Map.FullGroup)
            {
                if (avaragePoint.IsCreated) avaragePoint.Dispose();
                if (avarageDistances.IsCreated) avarageDistances.Dispose();
                if (minDistances.IsCreated) minDistances.Dispose();
                if (maxDistances.IsCreated) maxDistances.Dispose();
                if (groupSize.IsCreated) groupSize.Dispose();

                avaragePoint = new NativeArray<float3>(Map.FullGroup, Allocator.Persistent);
                avarageDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
                minDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
                maxDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
                groupSize = new NativeArray<int>(Map.FullGroup, Allocator.Persistent);
            }

            for (int i = 0; i < Map.FullGroup; i++)
            {
                avarageDistances[i] = 0;
                avaragePoint[i] = float3.zero;
                minDistances[i] = float.MaxValue;
                maxDistances[i] = 0;
                groupSize[i] = 0;
            }

            Entities.ForEach((ref GroupCondition condition, ref Walker walker, ref Translation translation) =>
            {
                var dist = math.length(translation.Value - condition.goal);
                if (minDistances[walker.broId] > dist) minDistances[walker.broId] = dist;
                if (maxDistances[walker.broId] < dist) maxDistances[walker.broId] = dist;
                avarageDistances[walker.broId] += dist;
                avaragePoint[walker.broId] += translation.Value;
                groupSize[walker.broId]++;
            });

            for (int i = 0; i < Map.FullGroup; i++)
            {
                if (groupSize[i] > 0)
                {
                    avarageDistances[i] /= (float)groupSize[i];
                    avaragePoint[i] /= (float)groupSize[i];
                }
            }
        }
    }
}
