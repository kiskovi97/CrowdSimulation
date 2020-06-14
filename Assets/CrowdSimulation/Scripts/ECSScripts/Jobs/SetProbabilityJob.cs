using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Burst;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct SetProbabilityJob : IJobForEach<Translation, Walker, CollisionParameters>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<float> quadrantHashMap;

        public int oneLayer;
        public MapValues max;

        public void Execute([ReadOnly]ref Translation translation, [ReadOnly] ref Walker walker, [ReadOnly] ref CollisionParameters collisionParameters)
        {
            float3 pos = translation.Value;
            var step = math.length(DensitySystem.Up);
            var max = collisionParameters.outerRadius * Map.density;

            for (float i = -max; i < max; i += step)
                for (float j = -max; j < max; j += step)
                {
                    Add(pos + DensitySystem.Up * i + DensitySystem.Right * j, pos, max, walker.direction);
                }
        }

        private void Add(float3 position, float3 centerPos, float maxdistance, float3 velocity)
        {
            var keyDistance = QuadrantVariables.IndexFromPosition(position, centerPos, max);
            if (keyDistance.key < 0)
            {
                return;
            }
            quadrantHashMap[keyDistance.key] += Value(maxdistance, centerPos, keyDistance.roundedPosition, velocity);
        }

        public static float Value(float maxDistance, float3 center, float3 pos, float3 velocity)
        {
            var direction = pos - center;
            var dot = math.dot(math.normalizesafe(direction), math.normalizesafe(velocity));
            var newPos = pos - math.pow(dot, 2f) * velocity * 0.3f;
            var distance = math.length(pos - center);
            var value = math.max(0f, (maxDistance - distance) / maxDistance);
            return math.pow(value, 3f);
        }
    }
}
