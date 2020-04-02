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
    }
}

