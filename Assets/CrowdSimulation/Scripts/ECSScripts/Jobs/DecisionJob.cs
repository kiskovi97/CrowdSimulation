using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct DecisionJob : IJobForEach<GroupForce, DesireForce, DecidedForce>
{
    public void Execute(ref GroupForce groupForce, ref DesireForce desireForce, ref DecidedForce decidedForce)
    {
    }
}
