using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct GroupGoalJob : IJobChunk// IJobForEach<Translation, GroupCondition>
    {
        public NativeArray<float3> avaragePoints;
        public NativeArray<float> avarageDistances;
        public NativeArray<float> minDistances;
        public NativeArray<float> maxDistances;

        [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<GroupCondition> GroupConditionHandle;
        [ReadOnly] public ComponentTypeHandle<Walker> WalkerHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var groupCondtitions = chunk.GetNativeArray(GroupConditionHandle);
            var walkers = chunk.GetNativeArray(WalkerHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var groupCondtition = groupCondtitions[i];
                var translation = translations[i];
                var walker = walkers[i];

                Execute(ref translation, ref groupCondtition, ref walker);

                groupCondtitions[i] = groupCondtition;
            }
        }
        public void Execute([ReadOnly] ref Translation translation, ref GroupCondition group, ref Walker walker)
        {
            var force = (group.goalPoint - translation.Value);
            var xForce = (group.goal - translation.Value);

            var avarageDistance = avarageDistances[walker.broId];
            var avaragePoint = avaragePoints[walker.broId];
            var minDistance = minDistances[walker.broId];
            var maxDistance = maxDistances[walker.broId];

            var direction = math.normalize(translation.Value - avaragePoint);

            //var percentage = (math.length(xForce) - minDistance) / (maxDistance - minDistance);

            //var rotated = math.rotate(quaternion.AxisAngle(new float3(0, 1, 0), percentage * 45), math.normalize(force)) * group.goalRadius * 1.5f;

            group.goal = group.goalPoint + direction * group.goalRadius * 1.5f;

            //if (avarageDistance < math.length(force) || math.length(xForce) < group.goalRadius * 1.7f)
            //{
            //    group.goal = group.goalPoint - math.normalize(force) * group.goalRadius * 1.5f;
            //}
            //else
            //{
            //    group.goal = group.goalPoint + math.normalize(force) * group.goalRadius * 1.5f;
            //}

            group.isSet = true;
            group.radius = 0.1f;
        }
    }
}
