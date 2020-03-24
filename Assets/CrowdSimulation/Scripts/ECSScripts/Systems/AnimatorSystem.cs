using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class AnimatorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;
        var job = new AnimatorJob() { deltaTime = deltaTime };
        return job.Schedule(this, inputDeps);
    }
}
