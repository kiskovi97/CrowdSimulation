using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct HurtingJob : IJobForEach<Fighter, Condition, Translation, CollisionParameters>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

        public float deltaTime;

        public void Execute(ref Fighter fighter, ref Condition condition, [ReadOnly] ref Translation translation, [ReadOnly] ref CollisionParameters collisionParameters)
        {
            condition.hurting -= deltaTime;
            condition.hurting = math.max(condition.hurting, 0f);

            if (condition.hurting <= 0f && condition.lifeLine < condition.maxLifeLine)
            {
                condition.lifeLine += deltaTime * condition.healingSpeed;
            }

            ForeachAround(translation.Value, fighter, ref condition);
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

