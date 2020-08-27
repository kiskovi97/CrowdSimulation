using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(EntitiesHashMap))]
    [UpdateAfter(typeof(CollidersHashMap))]
    public class CollisionSystem : ComponentSystem
    {
        private EntityQuery collisionResolveGroup;
        protected override void OnCreate()
        {
            var collisionQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Translation), typeof(Walker), typeof(CollisionParameters) },
            };
            collisionResolveGroup = GetEntityQuery(collisionQuery);

            base.OnCreate();
        }
        protected override void OnUpdate()
        {
            var collisionForce = new CollisionResolveJob()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
                colliders = CollidersHashMap.quadrantHashMap,
                deltaTime = Time.DeltaTime,
                CollisionParametersHandle = GetComponentTypeHandle<CollisionParameters>(),
                WalkerHandle = GetComponentTypeHandle<Walker>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
            };
            var collisionHandle = collisionForce.Schedule(collisionResolveGroup);
            collisionHandle.Complete();
        }
    }
}
