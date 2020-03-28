using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(PathFindingSystem))]
[UpdateAfter(typeof(CollisionSystem))]
public class WalkingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        var forceJob = new ForceJob() { deltaTime = deltaTime };
        var forceHandle = forceJob.Schedule(this, inputDeps);

        var walker = new WalkerJob() { deltaTime = deltaTime, maxWidth = Map.maxWidth, maxHeight = Map.maxHeight };
        var walkerHandle = walker.Schedule(this, forceHandle);

        return walkerHandle;
    }
}
