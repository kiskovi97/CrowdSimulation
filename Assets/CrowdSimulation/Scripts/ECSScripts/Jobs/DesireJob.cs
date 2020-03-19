using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct DesireJob : IJobForEach<Translation, Condition, FoodHierarchie, DesireForce>
{
    public void Execute([ReadOnly] ref Translation translation, [ReadOnly]  ref Condition condition, [ReadOnly] ref FoodHierarchie foodHierarchie, ref DesireForce desireForce)
    {
    }
}
