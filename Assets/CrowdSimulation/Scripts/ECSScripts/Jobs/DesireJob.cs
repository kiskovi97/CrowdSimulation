using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct DesireJob : IJobForEachWithEntity<Translation, Condition, FoodHierarchie, DesireForce, Walker>
    {
        private static readonly float secondPerHunger = 60f;
        private static readonly float hungerLimit = 1f;


        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EdibleHashMap.MyData> targetMap;

        public EntityCommandBuffer.Concurrent commandBuffer;

        public float deltaTime;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation, ref Condition condition, [ReadOnly] ref FoodHierarchie foodHierarchie, ref DesireForce desireForce, ref Walker walker)
        {
            //condition.hunger += deltaTime / secondPerHunger;
            if (condition.hunger < hungerLimit)
            {
                desireForce.force = float3.zero;
                return;
            }

            bool found = false;
            EdibleHashMap.MyData foundFood = new HashMapBase<Edible>.MyData();
            ForeachAround(translation.Value, ref foundFood, ref found);

            if (found)
            {
                var length = math.length(foundFood.position - translation.Value);

                if (length < 0.4f)
                {
                    commandBuffer.DestroyEntity(index, foundFood.entity);
                    condition.hunger -= foundFood.data.nutrition;
                    if (condition.hunger < 0) condition.hunger = 0;
                    if (length < 0.01f)
                    {
                        desireForce.force = float3.zero;
                        return;
                    }
                }
                desireForce.force = math.normalize(foundFood.position - translation.Value);
                if (length < math.dot(desireForce.force, walker.direction))
                {
                    walker.direction *= 0.9f;
                }
            }
            else
            {
                desireForce.force = float3.zero;
            }
        }

        private void ForeachAround(float3 position, ref EdibleHashMap.MyData foundFood, ref bool found)
        {
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, position, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, position, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, position, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, position, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, position, ref foundFood, ref found);
        }

        private void Foreach(int key, float3 me, ref EdibleHashMap.MyData foundFood, ref bool found)
        {
            if (targetMap.TryGetFirstValue(key, out EdibleHashMap.MyData food, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (!found)
                    {
                        foundFood = food;
                        found = true;
                    }
                    else
                    {
                        var prev = math.lengthsq(foundFood.position - me);
                        var next = math.lengthsq(food.position - me);
                        if (next < prev)
                        {
                            foundFood = food;
                        }
                    }

                } while (targetMap.TryGetNextValue(out food, ref iterator));
            }
        }
    }
}
