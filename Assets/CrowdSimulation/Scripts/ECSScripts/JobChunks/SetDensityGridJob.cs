﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Burst;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct SetDensityGridJob : IJobChunk
    {

        [NativeDisableParallelForRestriction]
        public NativeArray<float> quadrantHashMap;

        public int oneLayer;
        public int maxGroup;
        public MapValues max;

        [ReadOnly] public ComponentTypeHandle<Walker> WalkerHandle;
        [ReadOnly] public ComponentTypeHandle<CollisionParameters> CollisionParametersHandle;
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerHandle);
            var collisions = chunk.GetNativeArray(CollisionParametersHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var collision = collisions[i];
                var translation = translations[i];

                Execute(ref translation, ref walker, ref collision);
            }
        }

        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref Walker walker, [ReadOnly] ref CollisionParameters collisionParameters)
        {
            float3 pos = translation.Value;
            var step = math.length(DensitySystem.Up);
            var max = collisionParameters.outerRadius;

            for (float i = -max; i < max; i += step)
                for (float j = -max; j < max; j += step)
                {
                    Add(pos + DensitySystem.Up * i + DensitySystem.Right * j, pos, max, walker.broId % StaticMap.maxGroup);
                }
        }

        private void Add(float3 position, float3 prev, float maxdistance, int gid)
        {
            var keyDistance = QuadrantVariables.IndexFromPosition(position, prev, max);
            if (keyDistance.key < 0)
            {
                return;
            }
            for (int group = 0; group < maxGroup; group++)
            {
                if (group != gid)
                {
                    quadrantHashMap[oneLayer * group + keyDistance.key] += math.max(0f, (maxdistance - keyDistance.distance) / maxdistance);
                }
            }
        }
    }
}
