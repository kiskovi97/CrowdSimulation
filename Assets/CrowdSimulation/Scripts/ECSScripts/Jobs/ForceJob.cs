using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public struct ForceJob : IJobForEach<PathForce, Walker>
{
    public float deltaTime;

    public void Execute(ref PathForce pathForce, ref Walker walker)
    {
        walker.direction += pathForce.force * deltaTime;
    }
}
