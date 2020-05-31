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

        public void Execute([ReadOnly] ref PathFindingData data, [ReadOnly] ref CollisionParameters collision,
            ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (!(data.avoidMethod == CollisionAvoidanceMethod.DensityGrid))
            {
                return;
            }

            var distance = data.decidedGoal - translation.Value;
            if (math.length(distance) < data.radius)
            {
                walker.force = data.decidedForce;
                return;
            }

            var group = oneLayer * walker.broId;

            var indexes = DensitySystem.IndexesFromPoisition(translation.Value, collision.outerRadius, max);
            // * math.length(walker.direction)

            var force = float3.zero;

            for (int i = 0; i < indexes.Length; i++)
            {
                var index = indexes[i].index;
                if (index < 0) continue;

                var density = densityMap[group + index];
                if (density > 0)
                    force += (translation.Value - indexes[i].position) * (density);
            }

            walker.force = force * 0.5f + data.decidedForce;
        }
    }
}
