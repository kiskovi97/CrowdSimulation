using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct DensityAvoidanceJob : IJobForEach<DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<float> densityMap;
    public void Execute(ref DecidedForce decidedForce, ref CollisionParameters collision, ref Walker walker, ref Translation translation, ref PathForce pathForce)
    {
        var group = Map.OneLayer * walker.broId;

        var indexes = DensitySystem.IndexesFromPoisition(translation.Value, collision.outerRadius * 2);

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
