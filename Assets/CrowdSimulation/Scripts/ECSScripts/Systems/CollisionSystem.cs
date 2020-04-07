using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(EntitiesHashMap))]
[UpdateAfter(typeof(CollidersHashMap))]
public class CollisionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var collisionForce = new CollisionResolveJob() {
            targetMap = EntitiesHashMap.quadrantHashMap,
            colliders = CollidersHashMap.quadrantHashMap,
        };
        var collisionHandle = collisionForce.Schedule(this);
        collisionHandle.Complete();
    }
}
