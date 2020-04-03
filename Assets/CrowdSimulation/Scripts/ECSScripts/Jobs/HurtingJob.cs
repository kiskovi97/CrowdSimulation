using Unity.Entities;
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
        ForeachAround(translation.Value, fighter.targetId, radius: collisionParameters.outerRadius, ref condition);
    }

    private void ForeachAround(float3 position, int targetId, float radius, ref Condition condition)
    {
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        Foreach(key, position, targetId, radius, ref condition);
        QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, position, targetId, radius, ref condition);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, position, targetId, radius, ref condition);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, position, targetId, radius, ref condition);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, position, targetId, radius, ref condition);
    }

    private void Foreach(int key, float3 position, int targetId, float radius, ref Condition condition)
    {
        if (targetMap.TryGetFirstValue(key, out FightersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                if (other.data2.broId != targetId) continue;
                var nowDistance = math.length(other.position - position);
                if (nowDistance < radius)
                {
                    condition.lifeLine -= deltaTime;
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}

