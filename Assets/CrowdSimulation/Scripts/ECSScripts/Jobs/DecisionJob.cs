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
            pathFindingData.decidedGoal = group.goalPoint;
        }
    }

    [BurstCompile]
    public struct SetDesireForceJob : IJobForEach<Condition, PathFindingData, Translation>
    {
        public void Execute([ReadOnly] ref Condition condition, ref PathFindingData pathFindingData, [ReadOnly]ref Translation translation)
        {
            pathFindingData.decidedGoal = condition.goal;
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

            if (conditionDistance < 0.2f)
            {
                if (groupDistance < 0.2f)
                {
                    pathFindingData.decidedGoal = translation.Value;
                    return;
                }
                pathFindingData.decidedGoal = group.goal;
                return;
            }
            if (groupDistance < 0.2f)
            {
                pathFindingData.decidedGoal = condition.goal;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (conditionDistance < groupDistance)
                    pathFindingData.decidedGoal = group.goal;
                else
                    pathFindingData.decidedGoal = condition.goal;
                return;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (conditionDistance < groupDistance)
                    pathFindingData.decidedGoal = condition.goal;
                else
                    pathFindingData.decidedGoal = group.goal;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Avarage)
            {
                pathFindingData.decidedGoal = (group.goal + condition.goal)/2f;
            }
        }
    }
}
