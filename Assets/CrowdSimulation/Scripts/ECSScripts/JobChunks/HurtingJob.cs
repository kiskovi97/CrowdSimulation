using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct HurtingJob : IJobChunk // IJobForEachWithEntity<Fighter, Condition, Translation, CollisionParameters>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

        [NativeDisableParallelForRestriction]
        public EntityCommandBuffer commandBuffer;

        public float deltaTime;

        public ComponentTypeHandle<Fighter> FighterHandle;
        public ComponentTypeHandle<Condition> ConditionHandle;
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;
        [ReadOnly] public EntityTypeHandle EntityHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var fighters = chunk.GetNativeArray(FighterHandle);
            var conditions = chunk.GetNativeArray(ConditionHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            var entities = chunk.GetNativeArray(EntityHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var fighter = fighters[i];
                var condition = conditions[i];
                var translation = translations[i];
                var entity = entities[i];


                Execute(entity, ref fighter, ref condition, ref translation);

                fighters[i] = fighter;
                conditions[i] = condition;
            }
        }

        public void Execute(Entity entity, ref Fighter fighter, ref Condition condition, 
            [ReadOnly] ref Translation translation)
        {
            condition.hurting -= deltaTime;
            condition.hurting = math.max(condition.hurting, 0f);

            if (condition.hurting <= 0f && condition.lifeLine < condition.maxLifeLine)
            {
                condition.lifeLine += deltaTime * condition.healingSpeed;
            }

            ForeachAround(translation.Value, fighter, ref condition);

            if (condition.lifeLine < 0f)
            {
                commandBuffer.DestroyEntity(entity);
            }
        }

        private void ForeachAround(float3 position, Fighter me, ref Condition condition)
        {
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, position, me, ref condition);
            QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, position, me, ref condition);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, position, me, ref condition);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, position, me, ref condition);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, position, me, ref condition);
        }

        private void Foreach(int key, float3 position, Fighter me, ref Condition condition)
        {
            if (targetMap.TryGetFirstValue(key, out FightersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (other.data.groupId == me.groupId) continue;
                    if (other.data.attack == AttackType.Mix)
                    {
                        var distance = math.length(other.position - position);
                        if (distance < other.data.attackRadius)
                        {
                            condition.lifeLine -= (other.data.targetId != me.Id ? deltaTime : deltaTime * 0.3f) * other.data.attackStrength;
                        }
                    }
                    else
                    {
                        if (other.data.attack == AttackType.One)
                        {
                            if (other.data.targetId != me.Id) continue;
                        }
                        var distance = math.length(other.position - position);
                        if (distance < other.data.attackRadius)
                        {
                            condition.lifeLine -= deltaTime * other.data.attackStrength;
                            condition.hurting = condition.hurtingTime;
                        }
                    }

                } while (targetMap.TryGetNextValue(out other, ref iterator));
            }
        }
    }
}

