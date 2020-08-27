using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(EdibleHashMap))]
    public class GoalSystem : ComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem endSimulation;
        private EntityQuery decisionGroup;
        private EntityQuery foodGroup;

        protected override void OnCreate()
        {
            endSimulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var query = new EntityQueryDesc
            {
                Any = new ComponentType[] { typeof(GroupCondition), typeof(Condition) },
                All = new ComponentType[] { typeof(PathFindingData), ComponentType.ReadOnly<Translation>() }
            };
            decisionGroup = GetEntityQuery(query);
            var foodQuery = new EntityQueryDesc
            {
                Any = new ComponentType[] { typeof(FoodHierarchie) },
                All = new ComponentType[] { typeof(Condition), typeof(Walker), ComponentType.ReadOnly<Translation>() }
            };
            foodGroup = GetEntityQuery(foodQuery);
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
                hierarchieMap = FoodHierarchieHashMap.quadrantHashMap,
                commandBuffer = endSimulation.CreateCommandBuffer().AsParallelWriter(),
                deltaTime = Time.DeltaTime,

                ConditionType = GetComponentTypeHandle<Condition>(false),
                FoodHieararchieType  = GetComponentTypeHandle<FoodHierarchie>(false),
                WalkerType = GetComponentTypeHandle<Walker>(false),
                TranslationType = GetComponentTypeHandle<Translation>(true),
            };
            var desireHandle = desireJob.Schedule(foodGroup);
            var groupGoalJob = new GroupGoalJob();
            var groupHandle = groupGoalJob.Schedule(this, desireHandle);

            var decisionJob = new DecisionJobChunk()
            {
                 ConditionType = GetComponentTypeHandle<Condition>(true),
                 GroupConditionType = GetComponentTypeHandle<GroupCondition>(true),
                 TranslationType = GetComponentTypeHandle<Translation>(true),
                 PathFindingType = GetComponentTypeHandle<PathFindingData>(false),
            };
            var decisionHandle = decisionJob.Schedule(decisionGroup, groupHandle);
            decisionHandle.Complete();
        }
    }
}
