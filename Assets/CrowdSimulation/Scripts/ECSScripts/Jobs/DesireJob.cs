using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct DesireJob : IJobForEach<Translation, Condition, FoodHierarchie, DesireForce>
{
    public void Execute([ReadOnly] ref Translation translation, [ReadOnly]  ref Condition condition, [ReadOnly] ref FoodHierarchie foodHierarchie, ref DesireForce desireForce)
    {
        desireForce.force = new float3(1, 0, 0);
    }
}
