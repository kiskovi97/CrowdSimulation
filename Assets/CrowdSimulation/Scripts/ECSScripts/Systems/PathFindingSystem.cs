using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(DensitySystem))]
[UpdateAfter(typeof(GoalSystem))]
[UpdateAfter(typeof(EntitiesHashMap))]
[UpdateAfter(typeof(FighterSystem))]
public class PathFindingSystem : ComponentSystem
{
     protected override void OnUpdate()
    {
        var avoidJob = new AvoidEverybody()
        {
            targetMap = EntitiesHashMap.quadrantHashMap,
        };
        var avoidHandle = avoidJob.Schedule(this);
        var pathFindingJob = new ForcePathFindingJob() {
            targetMap = EntitiesHashMap.quadrantHashMap,
        };
        var pathFindingHandle = pathFindingJob.Schedule(this, avoidHandle);
        var denistyAvoidanceJob = new DensityAvoidanceJob()
        {
            densityMap = DensitySystem.densityMatrix,
            oneLayer = Map.OneLayer,
            max = Map.Values
        };
        var handle = denistyAvoidanceJob.Schedule(this, pathFindingHandle);
        handle.Complete();
    }
}
