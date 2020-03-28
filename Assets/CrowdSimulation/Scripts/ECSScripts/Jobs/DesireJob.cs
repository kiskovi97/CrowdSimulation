using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct DesireJob : IJobForEach<Translation, Condition, FoodHierarchie, DesireForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, EdibleHashMap.MyData> targetMap;

    //private EntityCommandBuffer commandBuffer;

    public void Execute([ReadOnly] ref Translation translation, [ReadOnly]  ref Condition condition, [ReadOnly] ref FoodHierarchie foodHierarchie, ref DesireForce desireForce)
    {
        float3 closestPoint = translation.Value;
        bool found = false;
        ForeachAround(translation.Value, ref closestPoint, ref found);
        desireForce.force = math.normalize(closestPoint - translation.Value);
        //commandBuffer.DestroyEntity(entity);
    }

    private void ForeachAround(float3 position, ref float3 closestPoint, ref bool found)
    {
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        Foreach(key, position, ref closestPoint, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, position, ref closestPoint, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, position, ref closestPoint, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, position, ref closestPoint, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, position, ref closestPoint, ref found);
    }

    private void Foreach(int key, float3 me, ref float3 closestPoint, ref bool found)
    {
        if (targetMap.TryGetFirstValue(key, out EdibleHashMap.MyData food, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                if (!found)
                {
                    closestPoint = food.position;
                    found = true;
                }
                else
                {
                    var prev = math.lengthsq(closestPoint - me);
                    var next = math.lengthsq(food.position - me);
                    if (next < prev)
                    {
                        closestPoint = food.position;
                    }
                }

            } while (targetMap.TryGetNextValue(out food, ref iterator));
        }
    }
}
