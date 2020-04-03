using Unity.Entities;

[UpdateAfter(typeof(FightersHashMap))]
class FighterSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var job = new FighterJob()
        {
            targetMap = FightersHashMap.quadrantHashMap
        };
        var handle = job.Schedule(this);
        handle.Complete();
        var hJob = new HurtingJob()
        {
            targetMap = FightersHashMap.quadrantHashMap,
            deltaTime = Time.DeltaTime,
        };
        var hHandle = hJob.Schedule(this);
        hHandle.Complete();

    }
}

