using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct GroupGoalJob : IJobForEach<Translation, Walker,  GroupCondition, GroupForce>
{
    public void Execute(ref Translation translation, ref Walker walker, ref GroupCondition groupCondition, ref GroupForce group)
    {
        var force = (groupCondition.goalPoint - translation.Value);
        if (math.length(force) > groupCondition.goalRadius)
        {
            if (math.length(force) > 1f)
            {
                force = math.normalize(force);
            }
            group.force = force;
        } else
        {
            group.force = walker.direction * -1;
        }
    }
}
