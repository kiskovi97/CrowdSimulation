using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct GroupGoalJob : IJobForEach<Translation, GroupCondition>
    {
        public void Execute([ReadOnly] ref Translation translation, ref GroupCondition group)
        {
            var force = (group.goalPoint - translation.Value);
            if (math.length(force) > group.goalRadius)
            {
                group.goal = group.goalPoint;
            }
            else
            {
                group.goal = translation.Value;
            }
        }
    }
}
