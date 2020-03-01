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

        var pathFindingJob = new PathFindingJob() { targetMap = QuadrantSystem.quadrantHashMap };
        var pathHandle = pathFindingJob.Schedule(this, decisionHandle);

        var collisionForce = new CollisionJob() { targetMap = QuadrantSystem.quadrantHashMap };
        var collisionHandle = collisionForce.Schedule(this, pathHandle);

        var forceJob = new ForceJob() { deltaTime = deltaTime };
        var forceHandle = forceJob.Schedule(this, collisionHandle);

        var walker = new WalkerJob() { deltaTime = deltaTime };
        var walkerHandle = walker.Schedule(this, forceHandle);

        return walkerHandle;
    }
}
