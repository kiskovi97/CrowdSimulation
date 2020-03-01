using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct PathFindingJob : IJobForEach<DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, QuadrantData> targetMap;

    public void Execute(ref DecidedForce decidedForce, ref CollisionParameters collisionParameters, ref Walker walker, 
        ref Translation translation, ref PathForce pathForce)
    {
        var avoidanceForce = float3.zero;
        var convinientForce = float3.zero;
        ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value, broId = walker.broId }, 
            ref avoidanceForce, ref convinientForce, collisionParameters.outerRadius);

        pathForce.force = decidedForce.force + avoidanceForce;
        if (math.length(convinientForce) > 0.1f)
        {
            pathForce.force += math.normalize(convinientForce) * 0.5f;
        }
    }

    private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, float radius)
    {
        var position = me.position;
        var key = FlockingQuadrantSystem.GetPositionHashMapKey(position);
        Foreach(key, me, ref avoidanceForce, ref convinientForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, me, ref avoidanceForce, ref convinientForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, me, ref avoidanceForce, ref convinientForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, me, ref avoidanceForce, ref convinientForce, radius);
        key = FlockingQuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, me, ref avoidanceForce, ref convinientForce, radius);
    }


    private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, float radius)
    {
        if (targetMap.TryGetFirstValue(key, out QuadrantData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                if (me.broId == other.broId) {
                    convinientForce += other.direction;
                    continue;
                }

                var direction = me.position - other.position;
                var distance = math.length(direction);
                var distanceNormalized = (radius - distance) / (radius);

                if (distanceNormalized > 0f && distanceNormalized < 1f)
                {
                    var dot = (math.dot(math.normalize(-direction), math.normalize(me.direction)) + 1f) * 0.5f;

                    var forceMultiplyer = math.length(other.direction) + 0.7f;

                    var multiplyer = distanceNormalized * dot * forceMultiplyer;

                    var multiplyerSin = math.sin(multiplyer * math.PI / 2f);

                    avoidanceForce += math.normalize(other.direction) * multiplyerSin;

                    avoidanceForce += direction / radius;
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}
