using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct DesireJob : IJobForEachWithEntity<Translation, Condition, FoodHierarchie, DesireForce, Walker>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, EdibleHashMap.MyData> targetMap;

    public EntityCommandBuffer.Concurrent commandBuffer;

    public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, ref Condition condition, [ReadOnly] ref FoodHierarchie foodHierarchie, ref DesireForce desireForce, ref Walker walker)
    {
        if (condition.hunger < 0.1f) {
            desireForce.force = walker.direction * -1;
            return;
        }

        float3 closestPoint = translation.Value;
        bool found = false;
        Entity foundFoodEntity = Entity.Null;
        ForeachAround(translation.Value, ref closestPoint, ref foundFoodEntity, ref found);
        if (found)
        {
            var length = math.length(closestPoint - translation.Value);

            if (length < 0.4f)
            {
                commandBuffer.DestroyEntity(index, foundFoodEntity);
                condition.hunger -= 1f;
                if (condition.hunger < 0) condition.hunger = 0;
                if (length < 0.01f)
                {
                    desireForce.force = float3.zero;
                    return;
                }
            }
            desireForce.force =  math.normalize(closestPoint - translation.Value);
            if (length < math.dot(desireForce.force, walker.direction))
            {
                walker.direction *= 0.9f;
            }
        } else
        {
            desireForce.force = new float3(1, 0, 1);
        }
    }

    private void ForeachAround(float3 position, ref float3 closestPoint, ref Entity foundFoodEntity, ref bool found)
    {
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        Foreach(key, position, ref closestPoint, ref foundFoodEntity, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, position, ref closestPoint, ref foundFoodEntity, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, position, ref closestPoint, ref foundFoodEntity, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, position, ref closestPoint, ref foundFoodEntity, ref found);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, position, ref closestPoint, ref foundFoodEntity, ref found);
    }

    private void Foreach(int key, float3 me, ref float3 closestPoint, ref Entity foundFoodEntity, ref bool found)
    {
        if (targetMap.TryGetFirstValue(key, out EdibleHashMap.MyData food, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                if (!found)
                {
                    closestPoint = food.position;
                    foundFoodEntity = food.entity;
                    found = true;
                }
                else
                {
                    var prev = math.lengthsq(closestPoint - me);
                    var next = math.lengthsq(food.position - me);
                    if (next < prev)
                    {
                        closestPoint = food.position;
                        foundFoodEntity = food.entity;
                    }
                }

            } while (targetMap.TryGetNextValue(out food, ref iterator));
        }
    }
}
