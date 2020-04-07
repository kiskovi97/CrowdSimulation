using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct AvoidEverybody : IJobForEach<PathFindingData, DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, EntitiesHashMap.MyData> targetMap;


    public void Execute([ReadOnly]ref PathFindingData data, [ReadOnly]ref DecidedForce decidedForce, [ReadOnly]ref CollisionParameters collisionParameters, [ReadOnly]ref Walker walker,
         [ReadOnly]ref Translation translation, ref PathForce pathForce)
    {
        if (data.pathFindingMethod != PathFindingMethod.No)
        {
            return;
        }
        var avoidanceForce = float3.zero;
        ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value, broId = walker.broId },
            ref avoidanceForce, collisionParameters.innerRadius * 2);

        pathForce.force = decidedForce.force + avoidanceForce;
    }

    private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, float radius)
    {
        var position = me.position;
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        Foreach(key, me, ref avoidanceForce,radius);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, me, ref avoidanceForce, radius);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, me, ref avoidanceForce, radius);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, me, ref avoidanceForce, radius);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, me, ref avoidanceForce, radius);
    }

    private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce,float radius)
    {
        if (targetMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {

                var direction = me.position - other.position;
                var distance = math.length(direction);
                var distanceNormalized = (radius - distance) / (radius);

                if (distanceNormalized > 0f && distanceNormalized < 1f)
                {
                    var multiplyer = math.dot(math.normalizesafe(me.direction), distanceNormalized) + 1f;
                    avoidanceForce += direction / radius;
                }

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }
}

