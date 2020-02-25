using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct DesireJob : IJobForEach<Translation, Condition, FoodHierarchie, DesireForce>
{
    public void Execute(ref Translation translation, ref Condition condition, ref FoodHierarchie foodHierarchie, ref DesireForce desireForce)
    {
    }
}
