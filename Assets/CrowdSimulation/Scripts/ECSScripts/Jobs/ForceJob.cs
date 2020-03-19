using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ForceJob : IJobForEach<PathForce, Walker>
{
    public float deltaTime;

    public void Execute([ReadOnly] ref PathForce pathForce, ref Walker walker)
    {
        walker.direction += pathForce.force * deltaTime;

        var speed = math.length(walker.direction);

        if (speed > walker.maxSpeed)
        {
            walker.direction = math.normalize(walker.direction) * walker.maxSpeed;
        }
    }
}
