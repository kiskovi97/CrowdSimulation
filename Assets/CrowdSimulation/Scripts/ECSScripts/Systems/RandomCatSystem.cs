using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class RandomCatSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

        var forceJob = new RandomCatJob() {
            deltaTime = deltaTime,
            random = random
        };

        var forceHandle = forceJob.Schedule(this, inputDeps);

        var forceJob2 = new RandomCatGroupJob()
        {
            deltaTime = deltaTime,
            random = random
        };

        var forceHandle2 = forceJob2.Schedule(this, forceHandle);

        return forceHandle2;
    }
}
