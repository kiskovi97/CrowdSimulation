using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Burst;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct RandomCatJob : IJobChunk
    {
        public Random random;
        public float deltaTime;

        public ComponentTypeHandle<Walker> WalkerHandle;
        [ReadOnly] public ComponentTypeHandle<RandomCat> RandomCatHandle;
        public ComponentTypeHandle<Translation> TranslationHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerHandle);
            var randomCats = chunk.GetNativeArray(RandomCatHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var randomCat = randomCats[i];
                var translation = translations[i];

                Execute(ref randomCat, ref walker, ref translation);

                walkers[i] = walker;
                translations[i] = translation;
            }
        }

        public void Execute([ReadOnly] ref RandomCat randomCat, ref Walker walker, ref Translation translation)
        {
            if (random.NextFloat() < deltaTime * randomCat.random)
            {
                var dir = random.NextFloat2Direction();
                walker.force = new float3(dir.x, 0, dir.y);
            }
        }
    }

    [BurstCompile]
    public struct RandomCatGroupJob : IJobChunk
    {
        public Random random;
        public float deltaTime;

        public ComponentTypeHandle<Walker> WalkerHandle;
        [ReadOnly] public ComponentTypeHandle<RandomCat> RandomCatHandle;
        public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<GroupCondition> GroupConditionHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerHandle);
            var randomCats = chunk.GetNativeArray(RandomCatHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);
            var groupConditions = chunk.GetNativeArray(GroupConditionHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var randomCat = randomCats[i];
                var translation = translations[i];
                var groupCondition = groupConditions[i];

                Execute(ref randomCat, ref walker, ref translation, ref groupCondition);

                walkers[i] = walker;
                translations[i] = translation;
                groupConditions[i] = groupCondition;
            }
        }

        public void Execute([ReadOnly] ref RandomCat randomCat, ref Walker walker, ref Translation translation, ref GroupCondition groupCondition)
        {
            if (random.NextFloat() < deltaTime * randomCat.random)
            {

                var pos = groupCondition.goalPoint - translation.Value;

                var distance = math.length(pos);
                walker.force += deltaTime * distance * pos * 0.03f;
            }
        }
    }
}
