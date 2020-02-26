using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
[BurstCompile]
public struct GroupGoalJob : IJobForEach<Translation, GroupCondition, DesireForce>
{
    public void Execute(ref Translation translation, ref GroupCondition groupCondition, ref DesireForce desire)
    {
        var force = (groupCondition.goalPoint - translation.Value);
        if (math.length(force) > 1f)
        {
            force = math.normalize(force);
        }
        desire.force = groupCondition.goalPoint;
    }
}
