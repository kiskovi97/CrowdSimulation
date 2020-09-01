using Unity.Entities;
using Unity.Jobs;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(EdibleHashMap))]
    [UpdateAfter(typeof(GroupSystem))]
    public class GoalSystem : ComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem endSimulation;
        private EntityQuery decisionGroup;
        private EntityQuery foodGroup;
        private EntityQuery groupGroup;

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
            var groupQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(GroupCondition), typeof(Translation)}
            };
            groupGroup = GetEntityQuery(groupQuery);
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
            var groupGoalJob = new GroupGoalJob()
            {
                GroupConditionHandle = GetComponentTypeHandle<GroupCondition>(false),
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
                WalkerHandle = GetComponentTypeHandle<Walker>(true),
                avarageDistances = GroupSystem.avarageDistances,
                maxDistances = GroupSystem.maxDistances,
                minDistances = GroupSystem.minDistances,
                avaragePoints = GroupSystem.avaragePoint
            };
            var groupHandle = groupGoalJob.Schedule(groupGroup, desireHandle);

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
