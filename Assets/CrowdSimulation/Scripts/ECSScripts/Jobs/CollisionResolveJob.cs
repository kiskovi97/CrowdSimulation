using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct CollisionResolve : IJobForEach<Translation, Walker, CollisionParameters>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, QuadrantData> targetMap;

    public void Execute(ref Translation translation, ref Walker walker, ref CollisionParameters collision)
    {
        float3 correction = float3.zero;
        ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value , radius = collision.innerRadius},
            ref correction);
        translation.Value += correction * 0.1f;
        if (math.length(correction) > 0.1f)
        {
            walker.direction += correction;
        }
    }

    private void ForeachAround(QuadrantData me, ref float3 correction)
    {
        var position = me.position;
        var key = QuadrantSystem.GetPositionHashMapKey(position);
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, me, ref correction);


        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 1));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 1));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, -1));
        Foreach(key, me, ref correction);
        key = QuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, -1));
        Foreach(key, me, ref correction);
    }


    private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce)
    {
        if (targetMap.TryGetFirstValue(key, out QuadrantData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                var direction = me.position - other.position;
                var length = math.length(direction);
                if (length > 0.1f)
                {
                    var distance = math.max(0f, me.radius + other.radius - length);
                    avoidanceForce += distance * math.normalize(direction);
                }


            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}
