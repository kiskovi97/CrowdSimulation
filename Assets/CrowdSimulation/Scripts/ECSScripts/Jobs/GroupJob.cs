using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct GroupGoalJob : IJobForEach<Translation, Walker,  GroupCondition, GroupForce>
{
    public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref Walker walker, [ReadOnly] ref GroupCondition groupCondition, ref GroupForce group)
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
