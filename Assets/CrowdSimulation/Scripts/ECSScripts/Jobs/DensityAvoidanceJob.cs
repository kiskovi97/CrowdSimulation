﻿using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct DensityAvoidanceJob : IJobForEach<PathFindingData,DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<float> densityMap;

    public int oneLayer;

    public void Execute([ReadOnly]ref PathFindingData data, [ReadOnly] ref DecidedForce decidedForce, [ReadOnly] ref CollisionParameters collision, 
        [ReadOnly] ref Walker walker, [ReadOnly] ref Translation translation, ref PathForce pathForce)
    {
        if (!(data.method == PathFindingMethod.DensityGrid))
        {
            if (data.method == PathFindingMethod.No)
            {
                pathForce.force = decidedForce.force;
            }
            return;
        }

        var group = oneLayer * walker.broId;

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
