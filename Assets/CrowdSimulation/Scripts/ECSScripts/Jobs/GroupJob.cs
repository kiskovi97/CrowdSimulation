using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
[BurstCompile]
public struct GroupGoalJob : IJobForEach<Translation, GroupCondition, DesireForce>
{
    public void Execute(ref Translation c0, ref GroupCondition c1, ref DesireForce c2)
    {
    }
}
