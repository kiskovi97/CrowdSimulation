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

            var force = data.decidedForce;
            var densityB = GetDensity(collision.outerRadius, translation.Value, translation.Value + force, walker.direction, data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance);
            //densityB *= 0.8f;
            for (int i = 2; i < 4; i++)
            {
                densityB *= 0.9f;
                var multi = math.pow(0.5f, i);
                var A = GetDirection(force, math.PI * multi);
                var B = GetDirection(force, 0);
                var C = GetDirection(force, -math.PI * multi);

                var densityA = GetDensity(collision.outerRadius, translation.Value, translation.Value + A, walker.direction,
                    data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance) * 0.5f;

                var densityC = GetDensity(collision.outerRadius, translation.Value, translation.Value + C, walker.direction,
                    data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance) * 0.5f;

                if (densityA > densityC && densityB > densityC)
                {
                    force = C * 0.8f;
                    densityB = densityC;
                }
                else
                {
                    if (densityB > densityA && densityC > densityA)
                    {
                        force = A * 0.8f;
                        densityB = densityA;
                    }
                    else
                    {
                        force = B;
                    }
                }


            }

            walker.force = force;
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
                return (density0 + density1 + density2 + density3);// - ownDens);
            }
            else
            {
                var density0 = porbabilityMap[index.Index0] * index.percent0;
                var density1 = porbabilityMap[index.Index1] * index.percent1;
                var density2 = porbabilityMap[index.Index2] * index.percent2;
                var density3 = porbabilityMap[index.Index3] * index.percent3;
                var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
                return (density0 + density1 + density2 + density3 - ownDens);
            }

        }

        public static float3 GetDirection(float3 direction, float radians)
        {
            var rotation = quaternion.RotateY(radians);
            return math.rotate(rotation, direction);
        }
    }
}
