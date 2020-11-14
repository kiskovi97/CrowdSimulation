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
    public class GroupSystem : ComponentSystem
    {
        public static NativeArray<DistanceData> distanceDatas;

        public struct DistanceData
        {
            public float3 center;
            public float avarageDistance;
            public float absMaxDistance;
            public float2 absMaxDistanceXZ;
            public float2 absAvarageDistanceXZ;
            public int groupSize;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            distanceDatas = new NativeArray<DistanceData>(Map.FullGroup, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            if (distanceDatas.IsCreated) distanceDatas.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (distanceDatas.Length < Map.FullGroup)
            {
                if (distanceDatas.IsCreated) distanceDatas.Dispose();

                distanceDatas = new NativeArray<DistanceData>(Map.FullGroup, Allocator.Persistent);
            }

            for (int i = 0; i < Map.FullGroup; i++)
            {
                distanceDatas[i] = new DistanceData()
                {
                    absMaxDistanceXZ = float2.zero,
                    absAvarageDistanceXZ = float2.zero,
                    avarageDistance = 0f,
                    center = float3.zero,
                    groupSize = 0,
                    absMaxDistance = 0f,
                };
            }

            Entities.ForEach((ref GroupCondition condition, ref Walker walker, ref Translation translation) =>
            {
                var data = distanceDatas[walker.broId];
                data.center += translation.Value;
                data.groupSize++;
                distanceDatas[walker.broId] = data;
            });

            for (int i = 0; i < Map.FullGroup; i++)
            {
                var data = distanceDatas[i];
                if (data.groupSize > 0)
                {
                    data.center /= (float)data.groupSize;
                }
                distanceDatas[i] = data;
            }

            Entities.ForEach((ref GroupCondition condition, ref Walker walker, ref Translation translation) =>
            {
                var data = distanceDatas[walker.broId];

                var direction = translation.Value - data.center;
                var distance = math.length(direction);

                data.absMaxDistance += distance / data.groupSize;
                if (data.absMaxDistance < distance)
                    data.avarageDistance = distance;

                var x = data.absMaxDistanceXZ.x;
                var z = data.absMaxDistanceXZ.y;
                if (x < math.abs(direction.x))
                    x = math.abs(direction.x);
                if (z < math.abs(direction.z))
                    z = math.abs(direction.z);
                data.absMaxDistanceXZ = new float2(x, z);
                data.absAvarageDistanceXZ += math.abs(new float2(direction.x, direction.z) / data.groupSize);

                distanceDatas[walker.broId] = data;
            });

            //Entities.ForEach((ref GroupCondition condition, ref Walker walker, ref Translation translation) =>
            //{
            //    var data = distanceDatas[walker.broId];
            //    var A = new float3(data.absAvarageDistanceXZ.x * 2, 0, data.absAvarageDistanceXZ.y * 2);
            //    var B = new float3(-data.absAvarageDistanceXZ.x * 2, 0, data.absAvarageDistanceXZ.y * 2);
            //    var C = new float3(data.absAvarageDistanceXZ.x * 2, 0, -data.absAvarageDistanceXZ.y * 2);
            //    var D = new float3(-data.absAvarageDistanceXZ.x * 2, 0, -data.absAvarageDistanceXZ.y * 2);
            //    var point1 = SquereFormationHelper.GetGoalPosition(condition.goalPoint, condition.goalRadius, A, data);
            //    var point2 = SquereFormationHelper.GetGoalPosition(condition.goalPoint, condition.goalRadius, B, data);
            //    var point3 = SquereFormationHelper.GetGoalPosition(condition.goalPoint, condition.goalRadius, C, data);
            //    var point4 = SquereFormationHelper.GetGoalPosition(condition.goalPoint, condition.goalRadius, D, data);

            //    DebugProxy.DrawLine(point2, point1, Color.black);
            //    DebugProxy.DrawLine(point3, point2, Color.black);
            //    DebugProxy.DrawLine(point4, point3, Color.black);
            //    DebugProxy.DrawLine(point1, point3, Color.black);
            //    DebugProxy.DrawLine(point2, point4, Color.black);
            //});
        }
    }
}
