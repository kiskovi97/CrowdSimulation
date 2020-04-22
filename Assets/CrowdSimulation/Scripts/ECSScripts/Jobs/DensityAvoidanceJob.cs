using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct DensityAvoidanceJob : IJobForEach<PathFindingData, CollisionParameters, Walker, Translation>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> densityMap;

        public int oneLayer;
        public MapValues max;

        public void Execute([ReadOnly] ref PathFindingData pathFindingData, [ReadOnly] ref CollisionParameters collision,
            ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (!(pathFindingData.pathFindingMethod == PathFindingMethod.DensityGrid))
            {
                return;
            }

            var group = oneLayer * walker.broId;

            var indexes = DensitySystem.IndexesFromPoisition(translation.Value, collision.outerRadius, max); 
            // * math.length(walker.direction)

            var force = float3.zero;
            var multiMin = float.MaxValue;

            for (int i = 0; i < indexes.Length; i++)
            {
                var index = indexes[i].index;
                if (index < 0) continue;

                var density = densityMap[group + index];
                var currentForce = indexes[i].position - translation.Value;
                density -= (math.dot(math.normalizesafe(pathFindingData.Force(translation.Value, walker.direction)), 
                    math.normalizesafe(currentForce)) + 1f) * 0.1f;

                if (multiMin > density)
                {
                    multiMin = density;
                    force = indexes[i].position - translation.Value;
                }
            }

            walker.force = force; // decidedForce.force +
        }
    }
}
