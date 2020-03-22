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
public struct DecisionJob : IJobForEach<GroupForce, DesireForce, DecidedForce>
{
    public void Execute([ReadOnly] ref GroupForce groupForce, [ReadOnly] ref DesireForce desireForce, ref DecidedForce decidedForce)
    {
        decidedForce.force = groupForce.force + desireForce.force;
    }
}
