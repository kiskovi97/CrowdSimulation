using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(DensitySystem))]
    [UpdateAfter(typeof(EntitiesHashMap))]
    [UpdateAfter(typeof(FighterSystem))]
    [UpdateAfter(typeof(GoalSystem))]
    [UpdateAfter(typeof(ShortestPathSystem))]
    [UpdateAfter(typeof(ProbabilitySystem))]
    [UpdateAfter(typeof(DijsktraSystem))]
    [UpdateAfter(typeof(CollisionSystem))]
    public class PathFindingSystem : ComponentSystem
    {
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
            var pathFindingJ = new PathFindingJob()
            {
                values = Map.Values,
                AStarMatrix = AStarMatrixSystem.densityMatrix,
                entitiesHashMap = EntitiesHashMap.quadrantHashMap,
                densityMap = DensitySystem.densityMatrix,
                porbabilityMap = ProbabilitySystem.densityMatrix,
                goalPoints = ShortestPathSystem.goalPoints,
                graphPoints = GraphSystem.graphPoints,
                shortestPath = DijsktraSystem.shortestPath,
                shapeGraph = GraphSystem.shapeGraph,
                PathFindingType = GetComponentTypeHandle<PathFindingData>(),
                TranslationType = GetComponentTypeHandle<Translation>(),
                WalkerType = GetComponentTypeHandle<Walker>(),
                CollisionType = GetComponentTypeHandle<CollisionParameters>(),
            };
            var pathfindingHandle = pathFindingJ.Schedule(pathfindingGroup);
            pathfindingHandle.Complete();
        }
    }
}
