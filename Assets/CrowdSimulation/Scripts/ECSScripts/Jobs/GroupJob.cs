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
    public struct GroupGoalJob : IJobForEach<Translation, Walker, GroupCondition>
    {
        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref Walker walker, ref GroupCondition group)
        {
            var force = (group.goalPoint - translation.Value);
            if (math.length(force) > group.goalRadius)
            {
                //if (math.length(force) > 1f)
                //{
                //    force = math.normalize(force);
                //}
                group.force = math.normalize(force);
            }
            else
            {
                group.force = float3.zero;// walker.direction * -0.9f;
            }
        }
    }
}
