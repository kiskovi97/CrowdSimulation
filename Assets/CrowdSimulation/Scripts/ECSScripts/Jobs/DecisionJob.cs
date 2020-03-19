using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct DecisionJob : IJobForEach<GroupForce, DesireForce, DecidedForce>
{
    public void Execute([ReadOnly] ref GroupForce groupForce, [ReadOnly] ref DesireForce desireForce, ref DecidedForce decidedForce)
    {
        decidedForce.force = groupForce.force + desireForce.force;
    }
}
