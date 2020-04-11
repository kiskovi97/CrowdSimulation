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
    public struct GroupGoalJob : IJobForEach<Translation, Walker, GroupCondition, GroupForce>
    {
        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref Walker walker, [ReadOnly] ref GroupCondition groupCondition, ref GroupForce group)
        {
            var force = (groupCondition.goalPoint - translation.Value);
            if (math.length(force) > groupCondition.goalRadius)
            {
                if (math.length(force) > 1f)
                {
                    force = math.normalize(force);
                }
                group.force = force;
            }
            else
            {
                group.force = float3.zero;// walker.direction * -0.9f;
            }
        }
    }
}
