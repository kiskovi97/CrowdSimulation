using Unity.Entities;
using Unity.Jobs;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(QuadrantSystem))]
public class CollisionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var collisionForce = new CollisionResolve() { targetMap = QuadrantSystem.quadrantHashMap };
        var collisionHandle = collisionForce.Schedule(this, inputDeps);

        return collisionHandle;
    }
}
