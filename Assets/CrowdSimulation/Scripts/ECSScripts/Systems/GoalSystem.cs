using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(EdibleHashMap))]
    public class GoalSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem endSimulation;
        private EntityQuery decisionGroup;

        protected override void OnCreate()
        {
            endSimulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var query = new EntityQueryDesc
            {
                Any = new ComponentType[] { typeof(GroupCondition), typeof(Condition) },
                All = new ComponentType[] { typeof(PathFindingData), ComponentType.ReadOnly<Translation>() }
            };
            decisionGroup = GetEntityQuery(query);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // Set Desires
            var desireJob = new FoodSearchingJob()
            {
                targetMap = EdibleHashMap.quadrantHashMap,
                commandBuffer = endSimulation.CreateCommandBuffer().ToConcurrent(),
                deltaTime = Time.DeltaTime
            };
            var desireHandle = desireJob.Schedule(this, inputDeps);
            var groupGoalJob = new GroupGoalJob();
            var groupHandle = groupGoalJob.Schedule(this, desireHandle);



            var gfJob = new SetGroupForceJob();
            var gfHandle = gfJob.Schedule(this, groupHandle);
            var dfJob = new SetDesireForceJob();
            var dfHandle = dfJob.Schedule(this, gfHandle);

            var decisionJob = new DecisiionJobChunk()
            {
                 ConditionType = GetArchetypeChunkComponentType<Condition>(true),
                 GroupConditionType = GetArchetypeChunkComponentType<GroupCondition>(true),
                 TranslationType = GetArchetypeChunkComponentType<Translation>(true),
                 PathFindingType = GetArchetypeChunkComponentType<PathFindingData>(false),
            };
            var decisionHandle = decisionJob.Schedule(decisionGroup, dfHandle);

            return decisionHandle;
        }
    }
}
