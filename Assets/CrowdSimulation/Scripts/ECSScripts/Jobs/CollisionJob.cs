using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct CollisionJob : IJobForEach<Translation, Walker, CollisionParameters, CollisionForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, QuadrantData> targetMap;

    public void Execute(ref Translation translation, ref Walker walker, 
        ref CollisionParameters collisionParameters, ref CollisionForce collisionForce)
    {
        var avoidanceForce = float3.zero;
        ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value },
            ref avoidanceForce, collisionParameters.innerRadius);

        collisionForce.force = avoidanceForce * 20f;
    }

    private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, float radius)
    {
        var position = me.position;
        var key = FlockingQuadrantSystem.GetPositionHashMapKey(position);
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, me, ref avoidanceForce, radius);


        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 1));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 1));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, -1));
        Foreach(key, me, ref avoidanceForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, -1));
        Foreach(key, me, ref avoidanceForce, radius);
    }


    private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce, float radius)
    {
        if (targetMap.TryGetFirstValue(key, out QuadrantData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                var direction = me.position - other.position;
                var distance = math.length(direction);
                if (distance > 0f && distance < radius)
                {
                    var distanceNormalized = (radius - distance) / (radius);

                    var frontMultiplyer = (math.dot(math.normalize(-direction), math.normalize(me.direction)) + 1f) * 0.5f;

                    var forceMultiplyer = math.length(other.direction) + 0.7f;

                    var multiplyer = distanceNormalized * frontMultiplyer * forceMultiplyer;

                    var multiplyerSin = math.sin(multiplyer * math.PI / 2f);

                    avoidanceForce += math.normalize(direction) * multiplyerSin;

                    avoidanceForce += direction / radius;
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}
