using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct CollisionJob : IJobForEach<Translation, CollisionForce, Walker>
{
    public void Execute(ref Translation translation, ref CollisionForce collision, ref Walker walker)
    {
    }
}
