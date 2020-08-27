using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct GroupGoalJob : IJobChunk// IJobForEach<Translation, GroupCondition>
    {
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<GroupCondition> GroupConditionHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var groupCondtitions = chunk.GetNativeArray(GroupConditionHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var groupCondtition = groupCondtitions[i];
                var translation = translations[i];

                Execute(ref translation, ref groupCondtition);

                groupCondtitions[i] = groupCondtition;
            }
        }
        public void Execute([ReadOnly] ref Translation translation, ref GroupCondition group)
        {
            var force = (group.goalPoint - translation.Value);
            if (math.length(force) > group.goalRadius)
            {
                group.goal = group.goalPoint;
                group.isSet = true;
            }
            else
            {
                group.isSet = false;
            }
        }
    }
}
