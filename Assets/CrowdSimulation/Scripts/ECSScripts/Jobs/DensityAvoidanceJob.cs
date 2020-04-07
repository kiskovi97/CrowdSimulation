using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using static Map;

[BurstCompile]
public struct DensityAvoidanceJob : IJobForEach<PathFindingData,DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<float> densityMap;

    public int oneLayer;
    public MapValues max;

    public void Execute([ReadOnly]ref PathFindingData data, [ReadOnly] ref DecidedForce decidedForce, [ReadOnly] ref CollisionParameters collision, 
        [ReadOnly] ref Walker walker, [ReadOnly] ref Translation translation, ref PathForce pathForce)
    {
        if (!(data.pathFindingMethod == PathFindingMethod.DensityGrid))
        {
            return;
        }

        var group = oneLayer * walker.broId;

        var indexes = DensitySystem.IndexesFromPoisition(translation.Value, collision.outerRadius * math.length(decidedForce.force), max);

        var force = float3.zero;
        var multiMin = float.MaxValue;

        for (int i = 0; i < indexes.Length; i++)
        {
            var index = indexes[i].index;
            if (index < 0) continue;

            var density = densityMap[group + index];
            var currentForce = indexes[i].position - translation.Value;
            density -= (math.dot(math.normalize(decidedForce.force), math.normalize(currentForce)) + 1f) * 0.1f;

            if (multiMin > density)
            {
                multiMin = density;
                force = indexes[i].position - translation.Value;
            }
        }

        pathForce.force = force; // decidedForce.force +
    }
}
