using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;
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
    public struct SetDesireForceJob : IJobForEach<DesireForce, PathFindingData>
    {
        public void Execute([ReadOnly] ref DesireForce desireFoce, ref PathFindingData pathFindingData)
        {
            pathFindingData.decidedForce = desireFoce.force;
        }
    }

    [BurstCompile]
    public struct DecisionJob : IJobForEach<GroupCondition, DesireForce, PathFindingData, Walker, Translation>
    {
        public void Execute([ReadOnly] ref GroupCondition group, [ReadOnly] ref DesireForce desireForce, 
            ref PathFindingData pathFindingData, [ReadOnly] ref Walker walker, [ReadOnly] ref Translation translation)
        {
            if (math.length(desireForce.force) == 0f)
            {
                if (math.length(group.goal - translation.Value) == 0f)
                {
                    pathFindingData.decidedForce = -walker.direction;
                    return;
                }
                pathFindingData.decidedForce = group.Force(translation.Value);
                return;
            }
            if (math.length(group.goal - translation.Value) == 0f)
            {
                pathFindingData.decidedForce = desireForce.force;
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (math.length(desireForce.force) < math.length(group.Force(translation.Value)))
                    pathFindingData.decidedForce = group.Force(translation.Value);
                else
                    pathFindingData.decidedForce = desireForce.force;
                return;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (math.length(desireForce.force) < math.length(group.Force(translation.Value)))
                    pathFindingData.decidedForce = desireForce.force;
                else
                    pathFindingData.decidedForce = group.Force(translation.Value);
                return;
            }

            if (pathFindingData.decisionMethod == DecisionMethod.Plus)
            {
                pathFindingData.decidedForce = group.Force(translation.Value) + desireForce.force;
            }
        }
    }
}
