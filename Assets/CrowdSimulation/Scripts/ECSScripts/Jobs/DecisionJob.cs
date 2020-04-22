using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct SetGroupForceJob : IJobForEach<GroupForce, PathFindingData>
    {
        public void Execute([ReadOnly] ref GroupForce groupForce, ref PathFindingData pathFindingData)
        {
            pathFindingData.force = groupForce.force;
        }
    }

    [BurstCompile]
    public struct SetDesireForceJob : IJobForEach<DesireForce, PathFindingData>
    {
        public void Execute([ReadOnly] ref DesireForce desireFoce, ref PathFindingData pathFindingData)
        {
            pathFindingData.force = desireFoce.force;
        }
    }

    [BurstCompile]
    public struct DecisionJob : IJobForEach<GroupForce, DesireForce, PathFindingData, Walker>
    {
        public void Execute([ReadOnly] ref GroupForce groupForce, [ReadOnly] ref DesireForce desireForce, ref PathFindingData pathFindingData, [ReadOnly] ref Walker walker)
        {
            if (math.length(desireForce.force) == 0f)
            {
                if (math.length(groupForce.force) == 0f)
                {
                    pathFindingData.force = -walker.direction;
                    return;
                }
                pathFindingData.force = groupForce.force;
                return;
            }
            if (math.length(groupForce.force) == 0f)
            {
                pathFindingData.force = desireForce.force;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (math.length(desireForce.force) < math.length(groupForce.force))
                    pathFindingData.force = groupForce.force;
                else
                    pathFindingData.force = desireForce.force;
                return;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (math.length(desireForce.force) < math.length(groupForce.force))
                    pathFindingData.force = desireForce.force;
                else
                    pathFindingData.force = groupForce.force;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Plus)
            {
                pathFindingData.force = groupForce.force + desireForce.force;
            }
        }
    }
}
