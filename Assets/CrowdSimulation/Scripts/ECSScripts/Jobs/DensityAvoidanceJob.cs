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
                data.decidedForce *= 0.5f;
            }

            var group = oneLayer * walker.broId;

            var force = float3.zero;

            for (int i = 0; i < Angels; i++)
            {
                var vector = GetDirection(walker.direction, i * math.PI * 2f / Angels) * collision.innerRadius;
                var index = DensitySystem.BilinearInterpolation(translation.Value + vector, max);

                var density0 = densityMap[group + index.Index0] * index.percent0;
                var density1 = densityMap[group + index.Index1] * index.percent1;
                var density2 = densityMap[group + index.Index2] * index.percent2;
                var density3 = densityMap[group + index.Index3] * index.percent3;
                var density = density0 + density1 + density2 + density3;

                density0 = densityMap[index.Index0] * index.percent0;
                density1 = densityMap[index.Index1] * index.percent1;
                density2 = densityMap[index.Index2] * index.percent2;
                density3 = densityMap[index.Index3] * index.percent3;
                var densityOwn = density0 + density1 + density2 + density3;
                if (density > 0)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (density);
                }
                if (densityOwn > 3)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (densityOwn - 3f);
                }
            }

            walker.force = force + data.decidedForce;
        }

        private float3 GetDirection(float3 direction, float radians)
        {
            var rotation = quaternion.RotateY(radians);
            return math.rotate(rotation, math.normalizesafe(direction));
        }
    }
}
