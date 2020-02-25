using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
public class CrowdSystem : JobComponentSystem
{

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        var walker = new WalkerJob()
        {
            deltaTime = deltaTime,
        };
        var walkerHandle = walker.Schedule(this, inputDeps);


        return walkerHandle;
    }
}
