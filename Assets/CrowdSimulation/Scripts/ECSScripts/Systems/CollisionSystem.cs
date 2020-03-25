using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(QuadrantSystem))]
public class CollisionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collisionForce = new CollisionResolveJob() {
            targetMap = EntitiesHashMap.quadrantHashMap,
            colliders = CollidersHashMap.quadrantHashMap,
        };
        var collisionHandle = collisionForce.Schedule(this, inputDeps);

        return collisionHandle;
    }
}
