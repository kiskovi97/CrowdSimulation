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
        private static readonly int Angels = 10;

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
                walker.force = data.decidedForce * 0.1f;
                return;
            }

            var group = oneLayer * walker.broId;

            var center = DensitySystem.IndexFromPosition(translation.Value, float3.zero, max);
            var force = float3.zero;
            var dens = densityMap[center.key];

            for (int i = 0; i < Angels; i++)
            {
                var vector = GetDirection(walker.direction, i * math.PI * 2f / Angels) * collision.innerRadius;
                var index = DensitySystem.IndexFromPosition(translation.Value + vector, float3.zero, max);
                if (index.key < 0) continue;

                var density = densityMap[group + index.key];
                if (density > 0)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (density);
                }
            }

            walker.force = force + data.decidedForce;
            if (dens > 3f)
            {
                walker.direction *= 3f / dens;
            }
        }

        private float3 GetDirection(float3 direction, float radians)
        {
            var rotation = quaternion.RotateY(radians);
            return math.rotate(rotation, math.normalizesafe(direction));
        }
    }
}
