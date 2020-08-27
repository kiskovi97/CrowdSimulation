using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(PathFindingSystem))]
    [UpdateAfter(typeof(CollisionSystem))]
    public class WalkingSystem : ComponentSystem
    {
        private EntityQuery walkingEntities;
        protected override void OnCreate()
        {
            var walkingQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Walker), typeof(Rotation), typeof(Translation) },
            };
            walkingEntities = GetEntityQuery(walkingQuery);

            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var deltaTime = math.min(Time.DeltaTime, 0.05f);

            var forceJob = new ForceJob() { 
                deltaTime = deltaTime,
                WalkerHandle = GetComponentTypeHandle<Walker>(),
            };
            var forceHandle = forceJob.Schedule(walkingEntities);

            var walkerJob = new WalkerJob() { 
                deltaTime = deltaTime,
                maxWidth = Map.MaxWidth, 
                maxHeight = Map.MaxHeight,
                WalkerHandle = GetComponentTypeHandle<Walker>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
                RotationHandle = GetComponentTypeHandle<Rotation>(),
            };
            var walkerHandle = walkerJob.Schedule(walkingEntities, forceHandle);
            walkerHandle.Complete();
        }
    }
}
