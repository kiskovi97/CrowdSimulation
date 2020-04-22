using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct SetGroupForceJob : IJobForEach<GroupCondition, PathFindingData, Translation>
    {
        public void Execute([ReadOnly] ref GroupCondition group, ref PathFindingData pathFindingData, [ReadOnly]ref Translation translation)
        {
            pathFindingData.decidedForce = group.Force(translation.Value);
        }
    }

    [BurstCompile]
    public struct SetDesireForceJob : IJobForEach<Condition, PathFindingData, Translation>
    {
        public void Execute([ReadOnly] ref Condition condition, ref PathFindingData pathFindingData, [ReadOnly]ref Translation translation)
        {
            pathFindingData.decidedForce = condition.Force(translation.Value);
        }
    }

    [BurstCompile]
    public struct DecisionJob : IJobForEach<GroupCondition, Condition, PathFindingData, Walker, Translation>
    {
        public void Execute([ReadOnly] ref GroupCondition group, [ReadOnly] ref Condition condition, 
            ref PathFindingData pathFindingData, [ReadOnly] ref Walker walker, [ReadOnly] ref Translation translation)
        {

            var conditionDistance = math.length(condition.goal - translation.Value);
            var groupDistance = math.length(group.goal - translation.Value);

            if (conditionDistance == 0f)
            {
                if (groupDistance == 0f)
                {
                    pathFindingData.decidedForce = -walker.direction;
                    return;
                }
                pathFindingData.decidedForce = group.Force(translation.Value);
                return;
            }
            if (groupDistance == 0f)
            {
                pathFindingData.decidedForce = condition.Force(translation.Value);
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (conditionDistance < groupDistance)
                    pathFindingData.decidedForce = group.Force(translation.Value);
                else
                    pathFindingData.decidedForce = condition.Force(translation.Value);
                return;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (conditionDistance < groupDistance)
                    pathFindingData.decidedForce = condition.Force(translation.Value);
                else
                    pathFindingData.decidedForce = group.Force(translation.Value);
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Plus)
            {
                pathFindingData.decidedForce = group.Force(translation.Value) + condition.Force(translation.Value);
            }
        }
    }
}
