using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        public MapValues values;
        [ReadOnly] public NativeList<float> AStarMatrix;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerType);
            var pathFindings = chunk.GetNativeArray(PathFindingType);
            var translations = chunk.GetNativeArray(TranslationType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var pathFindingData = pathFindings[i];
                var translation = translations[i];


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
                            break;
                        case CollisionAvoidanceMethod.FutureAvoidance:
                            break;
                        case CollisionAvoidanceMethod.Probability:
                            break;
                        case CollisionAvoidanceMethod.No:
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
    }
}
