using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct FoodSearchingJob : IJobChunk
    {
        private static readonly float secondPerHunger = 60f;
        private static readonly float hungerLimit = 1f;


        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EdibleHashMap.MyData> targetMap;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, FoodHierarchieHashMap.MyData> hierarchieMap;

        public EntityCommandBuffer.ParallelWriter commandBuffer;

        public float deltaTime;

        [ReadOnly] public ComponentTypeHandle<Translation> TranslationType;
        public ComponentTypeHandle<Walker> WalkerType;
        public ComponentTypeHandle<Condition> ConditionType;
        public ComponentTypeHandle<FoodHierarchie> FoodHieararchieType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerType);
            var conditions = chunk.GetNativeArray(ConditionType);
            var translations = chunk.GetNativeArray(TranslationType);

            if (chunk.Has(FoodHieararchieType))
            {
                var hieararchies = chunk.GetNativeArray(FoodHieararchieType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var walker = walkers[i];
                    var condition = conditions[i];
                    var translation = translations[i];
                    var hierarchie = hieararchies[i];

                    if (hierarchie.hierarchieNumber > 0)
                    {
                        Execute(chunkIndex, translation, hierarchie, ref condition, ref walker);
                    }
                    else
                    {
                        Execute(chunkIndex, translation, ref condition, ref walker);
                    }

                    walkers[i] = walker;
                    conditions[i] = condition;
                }
            }
            else
            {
                for (var i = 0; i < chunk.Count; i++)
                {
                    var walker = walkers[i];
                    var condition = conditions[i];
                    var translation = translations[i];

                    Execute(chunkIndex, translation, ref condition, ref walker);

                    walkers[i] = walker;
                    conditions[i] = condition;
                }
            }
        }

        private void Execute(int index, Translation translation, FoodHierarchie hierarchie, ref Condition condition, ref Walker walker)
        {
            if (condition.hunger < hungerLimit)
            {
                condition.isSet = false;
                return;
            }

            var found = false;
            var foundPrey = new FoodHierarchieHashMap.MyData();
            ForeachAround(translation.Value, condition, walker.direction, hierarchie, ref foundPrey, ref found);

            if (found)
            {
                var foundFood = new EdibleHashMap.MyData
                {
                    position = foundPrey.position,
                    data = new Edible()
                    {
                        nutrition = foundPrey.data.nutrition,
                    },
                    entity = foundPrey.entity
                };
                FoundFood(translation, foundFood, index, ref walker, ref condition);
            }
            else
            {
                condition.isSet = false;
            }
        }

        private void Execute(int index, Translation translation, ref Condition condition, ref Walker walker)
        {
            if (condition.hunger < hungerLimit)
            {
                condition.isSet = false;
                return;
            }

            bool found = false;
            EdibleHashMap.MyData foundFood = new HashMapBase<Edible>.MyData();
            ForeachAround(translation.Value, condition, walker.direction, ref foundFood, ref found);

            if (found)
            {
                var length = math.length(foundFood.position - translation.Value);

                if (length < math.dot(math.normalizesafe(foundFood.position - translation.Value), walker.direction))
                {
                    walker.direction *= 0.9f;
                }

                FoundFood(translation, foundFood, index, ref walker, ref condition);
            }
            else
            {
                condition.isSet = false;
            }
        }

        private void FoundFood(Translation translation, EdibleHashMap.MyData foundFood, int index, ref Walker walker, ref Condition condition)
        {
            if (IsInRadius(translation.Value, condition.eatingRadius, condition.viewAngle, walker.direction, foundFood.position))
            {
                commandBuffer.DestroyEntity(index, foundFood.entity);
                condition.hunger -= foundFood.data.nutrition;
                if (condition.hunger < 0) condition.hunger = 0;
                condition.isSet = false;
                return;
            }
            else
            {
                condition.goal = foundFood.position;
                condition.isSet = true;
            }
        }

        private void ForeachAround(float3 position, Condition condition, float3 direction, ref EdibleHashMap.MyData foundFood, ref bool found)
        {
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, position, condition, direction, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, position, condition, direction, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, position, condition, direction, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, position, condition, direction, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, position, condition, direction, ref foundFood, ref found);
        }

        private void ForeachAround(float3 position, Condition condition, float3 direction, FoodHierarchie hierarchie, ref FoodHierarchieHashMap.MyData foundFood, ref bool found)
        {
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, position, condition, direction, hierarchie, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, position, condition, direction, hierarchie, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, position, condition, direction, hierarchie, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, position, condition, direction, hierarchie, ref foundFood, ref found);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, position, condition, direction, hierarchie, ref foundFood, ref found);
        }

        private void Foreach(int key, float3 me, Condition condition, float3 direction, ref EdibleHashMap.MyData foundFood, ref bool found)
        {
            if (targetMap.TryGetFirstValue(key, out EdibleHashMap.MyData food, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    bool inRadius = IsInRadius(me, condition.viewRadius, condition.viewAngle, direction, food.position);
                    if (!inRadius) continue;
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

        private void Foreach(int key, float3 me, Condition condition, float3 direction, FoodHierarchie hierarchie, ref FoodHierarchieHashMap.MyData foundFood, ref bool found)
        {
            if (hierarchieMap.TryGetFirstValue(key, out FoodHierarchieHashMap.MyData food, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (food.data.hierarchieNumber >= hierarchie.hierarchieNumber) continue;
                    if (math.length(food.position - me) < 0.01f) continue;
                    bool inRadius = IsInRadius(me, condition.viewRadius, condition.viewAngle, direction, food.position);
                    if (!inRadius) continue;
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
                } while (hierarchieMap.TryGetNextValue(out food, ref iterator));
            }
        }

        private bool IsInRadius(float3 currentPosition, float radius, float viewAngle, float3 viewDirection, float3 searchedPosition)
        {
            var direction = searchedPosition - currentPosition;
            var length = math.length(direction);
            if (length > radius) return false;

            if (math.length(viewDirection) < 0.01f) return true;
            var normalized = math.normalizesafe(viewDirection);
            var normalizedSearch = math.normalizesafe(direction);

            var cosView = math.cos(math.radians(viewAngle / 2f));
            var dot = math.dot(normalized, normalizedSearch);

            return (dot > cosView);
        }
    }
}
