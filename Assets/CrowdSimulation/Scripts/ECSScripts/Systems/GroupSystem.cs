using Unity.Entities;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks.ForationHelpers;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    class GroupSystem : ComponentSystem
    {
        public static NativeArray<float3> avaragePoint;
        public static NativeArray<float> avarageDistances;
        public static NativeArray<float> minDistances;
        public static NativeArray<float3> maxDistances;
        public static NativeArray<int> groupSize;

        protected override void OnCreate()
        {
            base.OnCreate();
            avaragePoint = new NativeArray<float3>(Map.FullGroup, Allocator.Persistent);
            avarageDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
            minDistances = new NativeArray<float>(Map.FullGroup, Allocator.Persistent);
            maxDistances = new NativeArray<float3>(Map.FullGroup, Allocator.Persistent);
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
                maxDistances = new NativeArray<float3>(Map.FullGroup, Allocator.Persistent);
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
                avaragePoint[walker.broId] += translation.Value;
                groupSize[walker.broId]++;
            });

            for (int i = 0; i < Map.FullGroup; i++)
            {
                if (groupSize[i] > 0)
                {
                    avaragePoint[i] /= (float)groupSize[i];
                }
            }

            Entities.ForEach((ref GroupCondition condition, ref Walker walker, ref Translation translation) =>
            {
                var dist = math.length(translation.Value - avaragePoint[walker.broId]);
                var distx = math.length(translation.Value.x - avaragePoint[walker.broId].x);
                var distz = math.length(translation.Value.z - avaragePoint[walker.broId].z);
                if (minDistances[walker.broId] > dist) minDistances[walker.broId] = dist;
                var maxDistance = maxDistances[walker.broId];
                if (maxDistance.y < dist) maxDistance.y = dist;
                if (maxDistance.x < distx) maxDistance.x = dist;
                if (maxDistance.z < distz) maxDistance.z = dist;
                maxDistances[walker.broId] = maxDistance;
                avarageDistances[walker.broId] += dist;

                var A = math.normalize(new float3(1, 0, 1));
                var B = math.normalize(new float3(-1, 0, 1));
                var C = math.normalize(new float3(1, 0, -1));
                var D = math.normalize(new float3(-1, 0, -1));
                var point = SquereFormationHelper.GetClosestPoint(condition.goalPoint, A, condition.goalRadius, 1f, groupSize[walker.broId], maxDistance);
                var point2 = SquereFormationHelper.GetClosestPoint(condition.goalPoint, B, condition.goalRadius, 1f, groupSize[walker.broId], maxDistance);
                var point3 = SquereFormationHelper.GetClosestPoint(condition.goalPoint, C, condition.goalRadius, 1f, groupSize[walker.broId], maxDistance);
                var point4 = SquereFormationHelper.GetClosestPoint(condition.goalPoint, D, condition.goalRadius, 1f, groupSize[walker.broId], maxDistance);

                DebugProxy.DrawLine(point2, point, Color.black);
                DebugProxy.DrawLine(point3, point2, Color.black);
                DebugProxy.DrawLine(point4, point3, Color.black);
                DebugProxy.DrawLine(point, point3, Color.black);
                DebugProxy.DrawLine(point2, point4, Color.black);
            });

            for (int i = 0; i < Map.FullGroup; i++)
            {
                if (groupSize[i] > 0)
                {
                    avarageDistances[i] /= (float)groupSize[i];
                }
            }
        }
    }
}
