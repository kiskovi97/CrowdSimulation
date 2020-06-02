using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;

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

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            iteration++;
            var shortestPathJob = new ShortestPathReadJob()
            {
                values = Map.Values,
                matrix = ShortestPathSystem.densityMatrix
            };
            var shortestHandle = shortestPathJob.Schedule(this);
            shortestHandle.Complete();

            var avoidJob = new AvoidEverybody()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
            };
            var avoidHandle = avoidJob.Schedule(this, shortestHandle);
            var pathFindingJob = new ForcePathFindingJob()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
            };
            var pathFindingHandle = pathFindingJob.Schedule(this, avoidHandle);
            var denistyAvoidanceJob = new DensityAvoidanceJob()
            {
                densityMap = DensitySystem.densityMatrix,
                oneLayer = Map.OneLayer,
                max = Map.Values
            };
            var finalHandle = denistyAvoidanceJob.Schedule(this, pathFindingHandle);
            var futureVoidanceJob = new FutureCollisionAvoidanceJob()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
                iteration = iteration,
            };
            var futureHandle = futureVoidanceJob.Schedule(this, finalHandle);
            var probabilityJob = new ProbabilityAvoidJob()
            {
                densityMap = DensitySystem.densityMatrix,
                max = Map.Values
            };
            var probabilityHandle = probabilityJob.Schedule(this, futureHandle);
            probabilityHandle.Complete();
        }
    }
}
