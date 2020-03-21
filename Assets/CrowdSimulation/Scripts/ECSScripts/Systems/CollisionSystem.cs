using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct MyCollider
{
    [ReadOnly]
    public PhysicsCollider collider;
    [ReadOnly]
    public LocalToWorld localToWorld;
}

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(QuadrantSystem))]
public class CollisionSystem : JobComponentSystem
{
    [NativeDisableParallelForRestriction]
    public static NativeArray<MyCollider> colliders;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(LocalToWorld), typeof(PhysicsCollider));
        var count = entityQuery.CalculateEntityCount();
        if (count != CollisionSystem.colliders.Length)
        {
            CollisionSystem.colliders.Dispose();
            CollisionSystem.colliders = new NativeArray<MyCollider>(count, Allocator.Persistent);
        }

        var locals = entityQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
        var colliders = entityQuery.ToComponentDataArray<PhysicsCollider>(Allocator.TempJob);
        for (int i=0; i< count; i++)
        {
            CollisionSystem.colliders[i] = new MyCollider()
            {
                localToWorld = locals[i],
                collider = colliders[i]
            };
        }
        locals.Dispose();
        colliders.Dispose();

        var collisionForce = new CollisionResolveJob() {
            targetMap = QuadrantSystem.quadrantHashMap,
            colliders = CollisionSystem.colliders
        };
        var collisionHandle = collisionForce.Schedule(this, inputDeps);

        return collisionHandle;
    }

    protected override void OnCreate()
    {
        colliders = new NativeArray<MyCollider>(0, Allocator.Persistent);
        base.OnCreate();
    }


    protected override void OnDestroy()
    {
        colliders.Dispose();
        base.OnDestroy();
    }
}
