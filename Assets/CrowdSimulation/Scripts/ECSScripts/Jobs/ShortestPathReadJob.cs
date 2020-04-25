using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    struct ShortestPathReadJob : IJobForEach<PathFindingData, Walker, Translation>
    {
        public MapValues values;
        public void Execute([ReadOnly] ref PathFindingData pathFindingData, ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (pathFindingData.pathFindingMethod != PathFindingMethod.ShortesPath) return;

            if (math.length(pathFindingData.decidedGoal - translation.Value) < ShortestPathSystem.minDistance)
            {
                walker.force = -walker.direction;
            }

            var minvalue = ShortestPathSystem.GetMinValue(translation.Value, values, pathFindingData.decidedGoal);

            if (math.length(minvalue.offsetVector) < 0.1f)
            {
                walker.force = -walker.direction;
            } else
            {
                walker.force = math.normalizesafe(minvalue.offsetVector);
            }

        }
    }
}
