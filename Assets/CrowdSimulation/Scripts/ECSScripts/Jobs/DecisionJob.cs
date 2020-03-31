using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct SetGroupForceJob : IJobForEach<GroupForce, DecidedForce>
{
    public void Execute([ReadOnly] ref GroupForce groupForce, ref DecidedForce decidedForce)
    {
        decidedForce.force = groupForce.force;
    }
}

[BurstCompile]
public struct SetDesireForceJob : IJobForEach<DesireForce, DecidedForce>
{
    public void Execute([ReadOnly] ref DesireForce desireFoce, ref DecidedForce decidedForce)
    {
        decidedForce.force = desireFoce.force;
    }
}

[BurstCompile]
public struct DecisionJob : IJobForEach<GroupForce, DesireForce, DecidedForce, PathFindingData>
{
    public void Execute([ReadOnly] ref GroupForce groupForce, [ReadOnly] ref DesireForce desireForce, ref DecidedForce decidedForce, ref PathFindingData pathFindingData)
    {
        if (math.length(desireForce.force) == 0f)
        {
            decidedForce.force = groupForce.force;
            return;
        }
        if (math.length(groupForce.force) == 0f)
        {
            decidedForce.force = desireForce.force;
            return;
        }

        if (pathFindingData.decisionMethod == DecisionMethod.Max)
        {
            if (math.length(desireForce.force) < math.length(groupForce.force))
                decidedForce.force = groupForce.force;
            else
                decidedForce.force = desireForce.force;
            return;
        }
        if (pathFindingData.decisionMethod == DecisionMethod.Min)
        {
            if (math.length(desireForce.force) < math.length(groupForce.force))
                decidedForce.force = desireForce.force;
            else
                decidedForce.force = groupForce.force;
            return;
        }

        if (pathFindingData.decisionMethod == DecisionMethod.Plus)
        {
            decidedForce.force = groupForce.force + desireForce.force;
        }
    }
}
