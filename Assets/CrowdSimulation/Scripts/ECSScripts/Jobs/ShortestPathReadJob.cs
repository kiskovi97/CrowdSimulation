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
        [ReadOnly] public NativeList<float> matrix;
        public void Execute(ref PathFindingData pathFindingData, [ReadOnly] ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (math.length(pathFindingData.decidedGoal - translation.Value) < pathFindingData.radius)
            {
                pathFindingData.decidedForce = -walker.direction;
                return;
            }

            if (pathFindingData.pathFindingMethod != PathFindingMethod.AStar)
            {
                pathFindingData.decidedForce = math.normalizesafe(pathFindingData.decidedGoal - translation.Value);
                return;
            }

            var minvalue = ShortestPathSystem.GetMinValue(translation.Value, values, pathFindingData.decidedGoal, matrix);

            var distance = math.length(minvalue.goalPoint - pathFindingData.decidedGoal);

            if (math.length(pathFindingData.decidedGoal - translation.Value) < pathFindingData.radius + distance)
            {
                pathFindingData.decidedForce = math.normalizesafe(pathFindingData.decidedGoal - translation.Value);
                return;
            }

            if (math.length(minvalue.offsetVector) < 0.01f)
            {
                pathFindingData.decidedForce = -walker.direction;
            } else
            {
                pathFindingData.decidedForce = math.normalizesafe(minvalue.offsetVector);
            }

        }
    }
}
