using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using static Assets.CrowdSimulation.Scripts.Utilities.CollisionCalculator;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    struct FutureCollisionAvoidanceJob : IJobForEachWithEntity<PathFindingData, CollisionParameters, Walker, Translation>
    {

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EntitiesHashMap.MyData> targetMap;

        public int iteration;

        private static readonly int IterationMax = 1;
        private static readonly float maxTime = 6f;


        public void Execute(Entity entity, int index, [ReadOnly]ref PathFindingData data,
           [ReadOnly]ref CollisionParameters collisionParameters, ref Walker walker, [ReadOnly]ref Translation translation)
        {
            if (!(data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance) || iteration % IterationMax != index % IterationMax)
            {
                return;
            }

            var avoidanceForce = float2.zero;
            var minTime = maxTime;
            ForeachAround(new Circle(translation.Value.xz, collisionParameters.innerRadius, walker.direction.xz),
                ref avoidanceForce, ref minTime);

            var distance = translation.Value - data.decidedGoal;
            if (math.length(distance) < data.radius)
            {
                walker.force = data.decidedForce * 0.1f;
                return;
            }

            walker.force = (data.decidedForce  + new float3(avoidanceForce.x, 0, avoidanceForce.y) * 5f) - walker.direction * (maxTime - minTime) / maxTime;
        }

        private void ForeachAround(Circle me, ref float2 avoidanceForce, ref float minTime)
        {
            var position = me.Position;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, me, ref avoidanceForce, ref minTime);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref minTime);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref minTime);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref avoidanceForce, ref minTime);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref avoidanceForce, ref minTime);
        }

        private void Foreach(int key, Circle me, ref float2 avoidanceForce, ref float minTime)
        {
            if (targetMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (math.lengthsq(other.position.xz - me.position) > maxTime * maxTime * 4f) continue;
                    var circleOther = new Circle(other.position.xz, other.data.innerRadius, other.data2.direction.xz);
                    var time = CalculateCirclesCollisionTime(me, circleOther);
                    if (time <= 0 || time > maxTime)
                    {
                        continue;
                    }
                    var avoidance = CalculateCollisionAvoidance(me, circleOther, time, maxTime);

                    if (minTime > time) minTime = time;
                    avoidanceForce += avoidance;

                } while (targetMap.TryGetNextValue(out other, ref iterator));
            }
        }
    }
}
