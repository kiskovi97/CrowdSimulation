using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(DensitySystem))]
    [UpdateAfter(typeof(GoalSystem))]
    [UpdateAfter(typeof(EntitiesHashMap))]
    [UpdateAfter(typeof(FighterSystem))]
    [UpdateAfter(typeof(ShortestPathSystem))]
    [UpdateAfter(typeof(ProbabilitySystem))]
    public class PathFindingSystem : ComponentSystem
    {
        private static int iteration = 0;

        private EntityQuery pathfindingGroup;

        protected override void OnCreate()
        {
            var pathFindingQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(PathFindingData), ComponentType.ReadOnly<Translation>(), typeof(Walker) },
            };
            pathfindingGroup = GetEntityQuery(pathFindingQuery);
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            iteration++;
            var pathFindingJ = new PathFindingJob()
            {
                values = Map.Values,
                AStarMatrix = ShortestPathSystem.densityMatrix,
                entitiesHashMap = EntitiesHashMap.quadrantHashMap,
                PathFindingType = GetArchetypeChunkComponentType<PathFindingData>(),
                TranslationType = GetArchetypeChunkComponentType<Translation>(),
                WalkerType = GetArchetypeChunkComponentType<Walker>(),
                CollisionType = GetArchetypeChunkComponentType<CollisionParameters>(),
            };
            var pathfindingHandle = pathFindingJ.Schedule(pathfindingGroup);
            pathfindingHandle.Complete();

            var denistyAvoidanceJob = new DensityAvoidanceJob()
            {
                densityMap = DensitySystem.densityMatrix,
                oneLayer = Map.OneLayer,
                max = Map.Values
            };
            var finalHandle = denistyAvoidanceJob.Schedule(this, pathfindingHandle);
            var probabilityJob = new ProbabilityAvoidJob()
            {
                densityMap = DensitySystem.densityMatrix,
                porbabilityMap = ProbabilitySystem.densityMatrix,
                max = Map.Values
            };
            var probabilityHandle = probabilityJob.Schedule(this, finalHandle);
            probabilityHandle.Complete();
        }
    }
}
