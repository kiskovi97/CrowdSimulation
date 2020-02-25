using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
public class CrowdSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        var groupGoalJob = new GroupGoalJob();
        var groupHandle = groupGoalJob.Schedule(this, inputDeps);

        var decisionJob = new DecisionJob();
        var decisionHandle = decisionJob.Schedule(this, groupHandle);

        var pathFindingJob = new PathFindingJob();
        var pathHandle = pathFindingJob.Schedule(this, decisionHandle);

        var forceJob = new ForceJob()
        {
            deltaTime = deltaTime,
        };
        var forceHandle = forceJob.Schedule(this, pathHandle);

        var walker = new WalkerJob()
        {
            deltaTime = deltaTime,
        };
        var walkerHandle = walker.Schedule(this, forceHandle);


        return walkerHandle;
    }
}
