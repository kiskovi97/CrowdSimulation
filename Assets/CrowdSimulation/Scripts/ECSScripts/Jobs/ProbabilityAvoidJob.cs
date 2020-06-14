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
    public struct ProbabilityAvoidJob : IJobForEach<PathFindingData, CollisionParameters, Walker, Translation>
    {
        public static readonly int Angels = 10;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> densityMap;


        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> porbabilityMap;

        public MapValues max;

        public void Execute([ReadOnly] ref PathFindingData data, [ReadOnly] ref CollisionParameters collision,
            ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (!(data.avoidMethod == CollisionAvoidanceMethod.Probability || data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance))
            {
                return;
            }

            var distance = data.decidedGoal - translation.Value;
            if (math.length(distance) < data.radius)
            {
                data.decidedForce *= 0.5f;
            }

            var force = float3.zero;

            for (int i = 0; i < Angels; i++)
            {
                var vector = GetDirection(walker.direction, i * math.PI * 2f / Angels) * collision.innerRadius;
                vector *= 1.0f;
                
                var density = GetDensity(collision.outerRadius, translation.Value, translation.Value + vector, walker.direction, 
                    data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance);

                if (density > 0)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (density);
                }
            }

            walker.force = force + data.decidedForce;
        }

        private float GetDensity(float radius, float3 position, float3 point, float3 velocity, bool dens)
        {
            var index = QuadrantVariables.BilinearInterpolation(point, max);

            if (dens)
            {
                var density0 = densityMap[index.Index0] * index.percent0;
                var density1 = densityMap[index.Index1] * index.percent1;
                var density2 = densityMap[index.Index2] * index.percent2;
                var density3 = densityMap[index.Index3] * index.percent3;
                //var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
                return (density0 + density1 + density2 + density3);
            } else
            {
                var density0 = porbabilityMap[index.Index0] * index.percent0;
                var density1 = porbabilityMap[index.Index1] * index.percent1;
                var density2 = porbabilityMap[index.Index2] * index.percent2;
                var density3 = porbabilityMap[index.Index3] * index.percent3;
                //var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
                return (density0 + density1 + density2 + density3);
            }
           
        }

        public static float3 GetDirection(float3 direction, float radians)
        {
            var rotation = quaternion.RotateY(radians);
            return math.rotate(rotation, math.normalizesafe(direction));
        }
    }
}
