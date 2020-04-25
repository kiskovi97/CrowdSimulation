using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var minvalue = ShortestPathSystem.GetMinValue(translation.Value, values, pathFindingData.decidedGoal);

            if (math.length(minvalue.offsetVector) < 0.1f)
            {
                walker.force = math.normalizesafe(pathFindingData.decidedGoal - translation.Value);
            } else
            {
                walker.force = math.normalizesafe(minvalue.offsetVector);
            }

        }
    }
}
