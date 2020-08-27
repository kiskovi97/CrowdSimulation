using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public class RandomCatSystem : JobComponentSystem
    {
        private EntityQuery randomCatEntities;
        private EntityQuery randomCatGroupEntities;
        protected override void OnCreate()
        {
            var walkingQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Walker), typeof(RandomCat), typeof(Translation) },
            };
            randomCatEntities = GetEntityQuery(walkingQuery);


            var groupQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Walker), typeof(RandomCat), typeof(Translation), typeof(GroupCondition) },
            };
            randomCatGroupEntities = GetEntityQuery(groupQuery);

            base.OnCreate();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var deltaTime = Time.DeltaTime;

            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

            var forceJob = new RandomCatJob()
            {
                deltaTime = deltaTime,
                random = random,
                RandomCatHandle = GetComponentTypeHandle<RandomCat>(true),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
                WalkerHandle = GetComponentTypeHandle<Walker>(),
            };

            var forceHandle = forceJob.Schedule(randomCatEntities, inputDeps);

            var forceJob2 = new RandomCatGroupJob()
            {
                deltaTime = deltaTime,
                random = random,
                RandomCatHandle = GetComponentTypeHandle<RandomCat>(true),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
                WalkerHandle = GetComponentTypeHandle<Walker>(),
                GroupConditionHandle = GetComponentTypeHandle<GroupCondition>(),
            };

            var forceHandle2 = forceJob2.Schedule(randomCatGroupEntities, forceHandle);

            return forceHandle2;
        }
    }
}
