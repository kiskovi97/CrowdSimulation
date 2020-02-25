using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ForceJob : IJobForEach<CollisionForce, PathForce, Walker>
{
    public float deltaTime;

    public void Execute(ref CollisionForce collisionForce, ref PathForce pathForce, ref Walker walker)
    {
        walker.direction += collisionForce.force * deltaTime;
        walker.direction += pathForce.force * deltaTime;
    }
}
