﻿using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Assets.CrowdSimulation.Scripts.Utilities;
using System.Globalization;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks.ForationHelpers;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct GroupGoalJob : IJobChunk// IJobForEach<Translation, GroupCondition>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<GroupSystem.DistanceData> distanceDatas;

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
            var data = distanceDatas[walker.broId];
            var center = data.center;
            var maxDistance = data.avarageDistance * 2f;
            var groupSize = data.groupSize;

            var direction = (translation.Value - center);
            var distance = math.length(direction);
            var directionNormal = math.normalize(direction);

            switch (group.formation)
            {
                case GroupFormation.Circel:
                    group.goal = GetCirclePoint(group.goalPoint, directionNormal, 
                        group.goalRadius * (group.fill ? 1.2f : 2f), 
                        group.fill ? distance / maxDistance : 1f);
                    break;
                case GroupFormation.Squere:

                    group.goal = SquereFormationHelper.GetGoalPosition(group.goalPoint, group.goalRadius, direction, data);
                    //group.goal = GetSquerePoint(group.goalPoint, direction,
                    //    group.goalRadius * (group.fill ? 1.41f * 1.1f : 2.5f), 
                    //    group.fill ? distance / maxDistance : 1f);
                    break;
            }            

            group.isSet = true;
            group.radius = 0.1f;
        }

        private float3 GetCirclePoint(float3 center, float3 direction, float radius, float percent)
        {
            return center + direction * radius * percent;
        }

        private float3 GetSquerePoint(float3 center, float3 direction, float radius, float percent)
        {

            float3 A = new float3();
            float3 B = new float3();
            float3 point = new float3();

            if (direction.x > 0f && direction.z > 0f)
            {
                A = center + new float3(1, 0, 0) * radius;
                B = center + new float3(0, 0, 1) * radius;
                point = MyMath.Intersect(center, center + direction * radius, A, B);
            }

            if (direction.x < 0f && direction.z > 0f)
            {
                A = center + new float3(-1, 0, 0) * radius;
                B = center + new float3(0, 0, 1) * radius;
                point = MyMath.Intersect(center, center + direction * radius, A, B);
            }

            if (direction.x < 0f && direction.z < 0f)
            {
                A = center + new float3(-1, 0, 0) * radius;
                B = center + new float3(0, 0, -1) * radius;
                point = MyMath.Intersect(center, center + direction * radius, A, B);
            }

            if (direction.x > 0f && direction.z < 0f)
            {
                A = center + new float3(1, 0, 0) * radius;
                B = center + new float3(0, 0, -1) * radius;
                point = MyMath.Intersect(center, center + direction * radius, A, B);
            }

            var final = point - center;
            var maxDistance = math.length(final);
            var finalPrecentage = math.min(percent * 1.3f, maxDistance / radius);

            return center + direction * radius * finalPrecentage;
        }
    }
}
