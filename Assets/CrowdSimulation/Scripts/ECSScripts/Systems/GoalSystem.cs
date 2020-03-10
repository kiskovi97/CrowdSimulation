using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
public class GoalSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var groupGoalJob = new GroupGoalJob();
        var groupHandle = groupGoalJob.Schedule(this, inputDeps);

        var decisionJob = new DecisionJob();
        var decisionHandle = decisionJob.Schedule(this, groupHandle);

        return decisionHandle;
    }
}
