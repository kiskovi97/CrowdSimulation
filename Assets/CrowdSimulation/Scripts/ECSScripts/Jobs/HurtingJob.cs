﻿using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public struct HurtingJob : IJobForEach<Fighter, Condition, Translation, CollisionParameters>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

    public float deltaTime;

    public void Execute(ref Fighter fighter, ref Condition condition, [ReadOnly] ref Translation translation, [ReadOnly] ref CollisionParameters collisionParameters)
    {
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
                    if (other.data.attack == AttackType.All)
                    {
                        if (other.data.targerGroupId != me.groupId) continue;
                    }
                    var distance = math.length(other.position - position);
                    if (distance < other.data.attackRadius)
                    {
                        condition.lifeLine -= deltaTime * other.data.attackStrength;
                    }
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}
