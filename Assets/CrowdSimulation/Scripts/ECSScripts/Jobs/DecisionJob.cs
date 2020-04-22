using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct SetGroupForceJob : IJobForEach<GroupCondition, PathFindingData>
    {
        public void Execute([ReadOnly] ref GroupCondition group, ref PathFindingData pathFindingData)
        {
            pathFindingData.decidedForce = group.force;
        }
    }

    [BurstCompile]
    public struct SetDesireForceJob : IJobForEach<DesireForce, PathFindingData>
    {
        public void Execute([ReadOnly] ref DesireForce desireFoce, ref PathFindingData pathFindingData)
        {
            pathFindingData.decidedForce = desireFoce.force;
        }
    }

    [BurstCompile]
    public struct DecisionJob : IJobForEach<GroupCondition, DesireForce, PathFindingData, Walker>
    {
        public void Execute([ReadOnly] ref GroupCondition group, [ReadOnly] ref DesireForce desireForce, ref PathFindingData pathFindingData, [ReadOnly] ref Walker walker)
        {
            if (math.length(desireForce.force) == 0f)
            {
                if (math.length(group.force) == 0f)
                {
                    pathFindingData.decidedForce = -walker.direction;
                    return;
                }
                pathFindingData.decidedForce = group.force;
                return;
            }
            if (math.length(group.force) == 0f)
            {
                pathFindingData.decidedForce = desireForce.force;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (math.length(desireForce.force) < math.length(group.force))
                    pathFindingData.decidedForce = group.force;
                else
                    pathFindingData.decidedForce = desireForce.force;
                return;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (math.length(desireForce.force) < math.length(group.force))
                    pathFindingData.decidedForce = desireForce.force;
                else
                    pathFindingData.decidedForce = group.force;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Plus)
            {
                pathFindingData.decidedForce = group.force + desireForce.force;
            }
        }
    }
}
