using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    struct PathFindingJob : IJobChunk
    {
        public ArchetypeChunkComponentType<PathFindingData> PathFindingType;
        public ArchetypeChunkComponentType<Walker> WalkerType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<CollisionParameters> CollisionType;
        
        public MapValues values;
        [ReadOnly] public NativeList<float> AStarMatrix;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EntitiesHashMap.MyData> entitiesHashMap;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerType);
            var pathFindings = chunk.GetNativeArray(PathFindingType);
            var translations = chunk.GetNativeArray(TranslationType);
            var collisions = chunk.GetNativeArray(CollisionType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var pathFindingData = pathFindings[i];
                var translation = translations[i];
                var collision = collisions[i];


                if (math.length(pathFindingData.decidedGoal - translation.Value) < pathFindingData.radius)
                {
                    pathFindingData.decidedForce = -walker.direction;
                }
                else
                {
                    switch (pathFindingData.pathFindingMethod)
                    {
                        case PathFindingMethod.AStar:
                            ExecuteAStar(ref pathFindingData, walker, translation);
                            break;
                        case PathFindingMethod.No:
                            pathFindingData.decidedForce = math.normalizesafe(pathFindingData.decidedGoal - translation.Value);
                            break;
                    }

                    switch (pathFindingData.avoidMethod)
                    {
                        case CollisionAvoidanceMethod.DensityGrid:
                            break;
                        case CollisionAvoidanceMethod.Forces:
                            ExecuteForceJob(pathFindingData, collision, ref walker, translation);
                            break;
                        case CollisionAvoidanceMethod.FutureAvoidance:
                            break;
                        case CollisionAvoidanceMethod.Probability:
                            break;
                        case CollisionAvoidanceMethod.No:
                            ExecuteAvoidEverybody(pathFindingData, collision, ref walker, translation);
                            break;
                    }
                }

                

                pathFindings[i] = pathFindingData;
                walkers[i] = walker;
            }
        }
        public void ExecuteAStar(ref PathFindingData pathFindingData, Walker walker, Translation translation)
        {
            var minvalue = ShortestPathSystem.GetMinValue(translation.Value, values, pathFindingData.decidedGoal, AStarMatrix);

            var distance = math.length(minvalue.goalPoint - pathFindingData.decidedGoal);

            if (math.length(pathFindingData.decidedGoal - translation.Value) < pathFindingData.radius + distance)
            {
                pathFindingData.decidedForce = math.normalizesafe(pathFindingData.decidedGoal - translation.Value);
                return;
            }

            if (math.length(minvalue.offsetVector) < 0.01f)
            {
                pathFindingData.decidedForce = -walker.direction;
            }
            else
            {
                pathFindingData.decidedForce = math.normalizesafe(minvalue.offsetVector);
            }

        }


        public void ExecuteAvoidEverybody(PathFindingData data, CollisionParameters collisionParameters, ref Walker walker, Translation translation)
        {
            var avoidanceForce = float3.zero;
            ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value, broId = walker.broId },
                ref avoidanceForce, collisionParameters.innerRadius * 2);

            walker.force = data.decidedForce + avoidanceForce;
        }

        private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, float radius)
        {
            var position = me.position;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, me, ref avoidanceForce, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref avoidanceForce, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref avoidanceForce, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref avoidanceForce, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref avoidanceForce, radius);
        }

        private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce, float radius)
        {
            if (entitiesHashMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var direction = me.position - other.position;
                    var distance = math.length(direction);
                    var distanceNormalized = (radius - distance) / (radius);
                    if (distanceNormalized > 0f && distanceNormalized < 1f)
                    {
                        avoidanceForce += direction / radius;
                    }

                } while (entitiesHashMap.TryGetNextValue(out other, ref iterator));
            }
        }

        public void ExecuteForceJob(PathFindingData data, CollisionParameters collisionParameters, ref Walker walker, Translation translation)
        {
            var avoidanceForce = float3.zero;
            var convinientForce = float3.zero;
            var bros = 0;
            ForeachAround(new QuadrantData() { direction = walker.direction, position = translation.Value, broId = walker.broId },
                ref avoidanceForce, ref convinientForce, ref bros, collisionParameters.outerRadius);

            var distance = translation.Value - data.decidedGoal;
            if (math.length(distance) < data.radius)
            {
                walker.force = data.decidedForce + avoidanceForce * 0.2f;
                return;
            }

            walker.force = data.decidedForce + avoidanceForce;

            if (bros > 0)
            {
                walker.force += convinientForce *= 1f / bros;
            }
        }

        private void ForeachAround(QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, ref int bros, float radius)
        {
            var position = me.position;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce, ref bros, radius);
        }

        private void Foreach(int key, QuadrantData me, ref float3 avoidanceForce, ref float3 convinientForce, ref int bros, float radius)
        {
            if (entitiesHashMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var direction = me.position - other.position;
                    var distance = math.length(direction);

                    if (me.broId == other.data2.broId && distance < 2 * radius)
                    {
                        convinientForce += other.data2.direction;
                        bros++;
                        distance *= 2f;
                    }

                    var distanceNormalized = (radius - distance) / (radius);

                    if (distanceNormalized > 0f && distanceNormalized < 1f)
                    {
                        var dot = (math.dot(math.normalizesafe(-direction), math.normalizesafe(me.direction)) + 1f) * 0.5f;

                        var forceMultiplyer = math.length(other.data2.direction) + 0.7f;

                        var multiplyer = distanceNormalized * dot * forceMultiplyer;

                        var multiplyerSin = math.sin(multiplyer * math.PI / 2f);

                        avoidanceForce += math.normalizesafe(other.data2.direction) * multiplyerSin;

                        avoidanceForce += direction / radius;
                    }

                } while (entitiesHashMap.TryGetNextValue(out other, ref iterator));
            }
        }
    }
}
