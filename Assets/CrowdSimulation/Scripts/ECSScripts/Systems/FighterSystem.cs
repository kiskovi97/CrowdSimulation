using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [UpdateAfter(typeof(FightersHashMap))]
    [UpdateAfter(typeof(GoalSystem))]
    class FighterSystem : ComponentSystem
    {
        public static NativeMultiHashMap<int, Fighter> hashMap;

        private EndSimulationEntityCommandBufferSystem endSimulation;

        private EntityQuery fighterEntities;
        protected override void OnCreate()
        {
            var walkingQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Walker), typeof(Translation), 
                    typeof(Fighter), typeof(Rotation), typeof(PathFindingData), typeof(Condition), },
            };
            fighterEntities = GetEntityQuery(walkingQuery);
           
            hashMap = new NativeMultiHashMap<int, Fighter>(0, Allocator.Persistent);
            endSimulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            hashMap.Dispose();
            base.OnDestroy();
        }

        [BurstCompile]
        public struct SetHashMapJob : IJobChunk
        {
            public NativeMultiHashMap<int, Fighter>.ParallelWriter hashMap;

            [ReadOnly] public ComponentTypeHandle<Fighter> FighterHandle;
            [ReadOnly] public EntityTypeHandle EntityHandle;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var fighters = chunk.GetNativeArray(FighterHandle);
                var entities = chunk.GetNativeArray(EntityHandle);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var fighter = fighters[i];
                    var entity = entities[i];

                    Execute(entity, ref fighter);
                }
            }

            public void Execute(Entity entity, ref Fighter fighter)
            {
                int key = entity.Index;
                hashMap.Add(key, fighter);
            }
        }

        protected override void OnUpdate()
        {
            var job = new FighterJob()
            {
                targetMap = FightersHashMap.quadrantHashMap,
                PathFindingDataHandle = GetComponentTypeHandle<PathFindingData>(),
                RotationHandle = GetComponentTypeHandle<Rotation>(),
                FighterHandle = GetComponentTypeHandle<Fighter>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
            };
            var handle = job.Schedule(fighterEntities);
            handle.Complete();
            var hJob = new HurtingJob()
            {
                targetMap = FightersHashMap.quadrantHashMap,
                deltaTime = Time.DeltaTime,
                commandBuffer = endSimulation.CreateCommandBuffer(),
                EntityHandle = GetEntityTypeHandle(),
                ConditionHandle = GetComponentTypeHandle<Condition>(),
                FighterHandle = GetComponentTypeHandle<Fighter>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
            };
            var hHandle = hJob.Schedule(fighterEntities);
            hHandle.Complete();

            EntityQuery entityQuery = GetEntityQuery(typeof(Fighter));
            hashMap.Clear();
            if (entityQuery.CalculateEntityCount() > hashMap.Capacity)
            {
                hashMap.Capacity = entityQuery.CalculateEntityCount();
            }

            var hashJob = new SetHashMapJob()
            {
                hashMap = hashMap.AsParallelWriter(),
                EntityHandle = GetEntityTypeHandle(),
                FighterHandle = GetComponentTypeHandle<Fighter>(true),
            };
            var hashHandle = hashJob.Schedule(entityQuery);
            hashHandle.Complete();

        }
    }
}

