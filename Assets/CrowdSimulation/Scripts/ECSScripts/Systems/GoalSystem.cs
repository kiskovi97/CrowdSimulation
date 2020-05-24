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
    public class GoalSystem : ComponentSystem
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

        protected override void OnUpdate()
        {
            // Set Desires
            var desireJob = new FoodSearchingJob()
            {
                targetMap = EdibleHashMap.quadrantHashMap,
                commandBuffer = endSimulation.CreateCommandBuffer().ToConcurrent(),
                deltaTime = Time.DeltaTime
            };
            var desireHandle = desireJob.Schedule(this);
            var groupGoalJob = new GroupGoalJob();
            var groupHandle = groupGoalJob.Schedule(this, desireHandle);

            var decisionJob = new DecisionJobChunk()
            {
                 ConditionType = GetArchetypeChunkComponentType<Condition>(true),
                 GroupConditionType = GetArchetypeChunkComponentType<GroupCondition>(true),
                 TranslationType = GetArchetypeChunkComponentType<Translation>(true),
                 PathFindingType = GetArchetypeChunkComponentType<PathFindingData>(false),
            };
            var decisionHandle = decisionJob.Schedule(decisionGroup, groupHandle);
            decisionHandle.Complete();
        }
    }
}
