using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(DensitySystem))]
[UpdateAfter(typeof(GoalSystem))]
[UpdateAfter(typeof(EntitiesHashMap))]
public class PathFindingSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var pathFindingJob = new PathFindingJob() {
            targetMap = EntitiesHashMap.quadrantHashMap,
        };
        var pathFindingHandle = pathFindingJob.Schedule(this, inputDeps);
        var denistyAvoidanceJob = new DensityAvoidanceJob()
        {
            densityMap = DensitySystem.densityMatrix,
            oneLayer = Map.OneLayer,
            max = Map.Values
        };
        var handle = denistyAvoidanceJob.Schedule(this, pathFindingHandle);

        return handle;
    }
}
