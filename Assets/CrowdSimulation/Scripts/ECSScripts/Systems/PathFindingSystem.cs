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
    public class PathFindingSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var avoidJob = new AvoidEverybody()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
            };
            var avoidHandle = avoidJob.Schedule(this);
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
            var densityHandle = denistyAvoidanceJob.Schedule(this, pathFindingHandle);

            var shortestPathJob = new ShortestPathReadJob()
            {
                values = Map.Values
            };
            var shortestHandle = shortestPathJob.Schedule(this, densityHandle);
            shortestHandle.Complete();
        }
    }
}
