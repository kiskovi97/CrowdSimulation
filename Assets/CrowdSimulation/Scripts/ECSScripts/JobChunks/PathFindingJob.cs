using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    struct PathFindingJob : IJobChunk
    {
        private static readonly int Angels = 10;

        public ArchetypeChunkComponentType<PathFindingData> PathFindingType;
        public ArchetypeChunkComponentType<Walker> WalkerType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkComponentType<CollisionParameters> CollisionType;

        public MapValues values;

        [ReadOnly] public NativeList<float> AStarMatrix;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EntitiesHashMap.MyData> entitiesHashMap;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> densityMap;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> porbabilityMap;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeList<float3> goalPoints;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float3> graphPoints;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<float> shortestPath;

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
                        case PathFindingMethod.Dijkstra:
                            ExecuteDijstkra(ref pathFindingData, walker, translation);
                            break;
                    }
                }

                switch (pathFindingData.avoidMethod)
                {
                    case CollisionAvoidanceMethod.DensityGrid:
                        ExecuteDensityGrid(pathFindingData, collision, ref walker, translation);
                        break;
                    case CollisionAvoidanceMethod.Forces:
                        ExecuteForceJob(pathFindingData, collision, ref walker, translation);
                        break;
                    case CollisionAvoidanceMethod.FutureAvoidance:
                        ExecuteProbability(pathFindingData, collision, ref walker, translation);
                        break;
                    case CollisionAvoidanceMethod.Probability:
                        ExecuteProbability(pathFindingData, collision, ref walker, translation);
                        break;
                    case CollisionAvoidanceMethod.No:
                        ExecuteAvoidEverybody(pathFindingData, collision, ref walker, translation);
                        break;
                }

                pathFindings[i] = pathFindingData;
                walkers[i] = walker;
            }
        }


        public void ExecuteDijstkra(ref PathFindingData data, Walker walker, Translation translation)
        {
            var offset = ClosestGoalPoint(data.decidedGoal) * graphPoints.Length;
            var min = 0;
            var minDistance = shortestPath[offset] + math.length(graphPoints[0] - translation.Value);
            for (int i=0; i<graphPoints.Length; i++)
            {
                if (shortestPath[offset + i] < 0) continue;

                var distance = shortestPath[offset + i] + math.length(graphPoints[i] - translation.Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    min = i;
                }
            }
            if (shortestPath[offset + min] < 0)
            {
                data.decidedForce = walker.direction * -1;
            }
            else
            {
                var goalPoint = graphPoints[min];
                data.decidedForce = goalPoint - translation.Value;
            }
            
        }

        public void ExecuteAStar(ref PathFindingData pathFindingData, Walker walker, Translation translation)
        {
            var minvalue = GetMinValue(translation.Value, values, pathFindingData.decidedGoal, AStarMatrix);
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

        public void ExecuteDensityGrid(PathFindingData data, CollisionParameters collision, ref Walker walker, Translation translation)
        {
            var distance = data.decidedGoal - translation.Value;
            if (math.length(distance) < data.radius)
            {
                data.decidedForce *= 0.5f;
            }

            var group = values.LayerSize * walker.broId;

            var force = float3.zero;

            for (int i = 0; i < Angels; i++)
            {
                var vector = GetDirection(walker.direction, i * math.PI * 2f / Angels) * collision.innerRadius;
                var index = QuadrantVariables.BilinearInterpolation(translation.Value + vector, values);

                var density0 = densityMap[group + index.Index0] * index.percent0;
                var density1 = densityMap[group + index.Index1] * index.percent1;
                var density2 = densityMap[group + index.Index2] * index.percent2;
                var density3 = densityMap[group + index.Index3] * index.percent3;
                var density = density0 + density1 + density2 + density3;

                density0 = densityMap[index.Index0] * index.percent0;
                density1 = densityMap[index.Index1] * index.percent1;
                density2 = densityMap[index.Index2] * index.percent2;
                density3 = densityMap[index.Index3] * index.percent3;
                var densityOwn = density0 + density1 + density2 + density3;
                if (density > 0)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (density);
                }
                if (densityOwn > 3)
                {
                    var direction = -vector / collision.outerRadius;
                    force += (math.normalizesafe(direction) - direction) * (densityOwn - 3f);
                }
            }

            walker.force = force + data.decidedForce;
        }

        public void ExecuteProbability(PathFindingData data, CollisionParameters collision, ref Walker walker, Translation translation)
        {
            var distance = data.decidedGoal - translation.Value;
            if (math.length(distance) < data.radius)
            {
                data.decidedForce *= 0.5f;
            }

            var force = data.decidedForce;
            var densityB = GetDensity(collision.outerRadius, translation.Value, translation.Value + force, walker.direction, data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance);

            for (int i = 2; i < 4; i++)
            {
                densityB *= 0.9f;
                var multi = math.pow(0.5f, i);
                var A = GetDirection(force, math.PI * multi);
                var B = GetDirection(force, 0);
                var C = GetDirection(force, -math.PI * multi);

                var densityA = GetDensity(collision.outerRadius, translation.Value, translation.Value + A, walker.direction,
                    data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance) * 0.5f;

                var densityC = GetDensity(collision.outerRadius, translation.Value, translation.Value + C, walker.direction,
                    data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance) * 0.5f;

                if (densityA > densityC && densityB > densityC)
                {
                    force = C * 0.8f;
                    densityB = densityC;
                }
                else
                {
                    if (densityB > densityA && densityC > densityA)
                    {
                        force = A * 0.8f;
                        densityB = densityA;
                    }
                    else
                    {
                        force = B;
                    }
                }
            }

            walker.force = force;
        }

        private float GetDensity(float radius, float3 position, float3 point, float3 velocity, bool dens)
        {
            var index = QuadrantVariables.BilinearInterpolation(point, values);

            if (dens)
            {
                var density0 = densityMap[index.Index0] * index.percent0;
                var density1 = densityMap[index.Index1] * index.percent1;
                var density2 = densityMap[index.Index2] * index.percent2;
                var density3 = densityMap[index.Index3] * index.percent3;
                //var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
                return (density0 + density1 + density2 + density3);// - ownDens);
            }
            else
            {
                var density0 = porbabilityMap[index.Index0] * index.percent0;
                var density1 = porbabilityMap[index.Index1] * index.percent1;
                var density2 = porbabilityMap[index.Index2] * index.percent2;
                var density3 = porbabilityMap[index.Index3] * index.percent3;
                var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
                return (density0 + density1 + density2 + density3 - ownDens);
            }

        }

        public static float3 GetDirection(float3 direction, float radians)
        {
            var rotation = quaternion.RotateY(radians);
            return math.rotate(rotation, direction);
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

        public AStarMatrixSystem.MinValue GetMinValue(float3 position, MapValues values, float3 goal, NativeList<float> matrix)
        {
            var index = QuadrantVariables.IndexFromPosition(position, position, values);
            if (goalPoints.Length <= 0)
            {
                return new AStarMatrixSystem.MinValue()
                {
                    index = index.key,
                    offsetVector = new float3(0, 0, 0),
                    value = 0f,
                    goalPoint = goal,
                };
            }
            var min = ClosestGoalPoint(goal);
            if (index.key < 0)
            {
                return new AStarMatrixSystem.MinValue()
                {
                    index = index.key,
                    offsetVector = new float3(0, 0, 0),
                    value = 0f,
                    goalPoint = goal,
                };
            }
            return GetMinValue(index.key + min * values.LayerSize, values, min, matrix);
        }

        private int ClosestGoalPoint(float3 point)
        {
            var min = 0;
            for (int i = 1; i < goalPoints.Length; i++)
            {
                if (math.lengthsq(goalPoints[min] - point) > math.lengthsq(goalPoints[i] - point))
                {
                    min = i;
                }
            }
            return min;
        }
    }
}
