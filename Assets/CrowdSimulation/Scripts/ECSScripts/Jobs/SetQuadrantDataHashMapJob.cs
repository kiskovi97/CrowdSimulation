using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct SetQuadrantDataHashMapJob : IJobForEach<Translation, Walker, CollisionParameters>
{
    public NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantHashMap;

    public void Execute([ReadOnly]ref Translation translation, [ReadOnly]ref Walker walker, [ReadOnly]ref CollisionParameters parameters)
    {
        int key = QuadrantVariables.GetPositionHashMapKey(translation.Value);
        quadrantHashMap.Add(key, new QuadrantData()
        {
            position = translation.Value,
            direction = walker.direction,
            broId = walker.broId,
            radius = parameters.innerRadius
        });
    }
}
