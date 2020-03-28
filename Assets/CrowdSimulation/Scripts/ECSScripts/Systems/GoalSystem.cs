using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(EdibleHashMap))]
public class GoalSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var desireJob = new DesireJob()
        {
            targetMap = EdibleHashMap.quadrantHashMap
        };
        var desireHandle = desireJob.Schedule(this, inputDeps);

        var groupGoalJob = new GroupGoalJob();
        var groupHandle = groupGoalJob.Schedule(this, desireHandle);

        var gfJob = new SetGroupForceJob();
        var gfHandle = gfJob.Schedule(this, groupHandle);
        var dfJob = new SetDesireForceJob();
        var dfHandle = dfJob.Schedule(this, gfHandle);
        var decisionJob = new DecisionJob();
        var decisionHandle = decisionJob.Schedule(this, dfHandle);

        return decisionHandle;
    }
}
