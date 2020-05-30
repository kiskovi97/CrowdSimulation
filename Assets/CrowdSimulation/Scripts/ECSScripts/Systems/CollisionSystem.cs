using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(EntitiesHashMap))]
    [UpdateAfter(typeof(CollidersHashMap))]
    public class CollisionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var collisionForce = new CollisionResolveJob()
            {
                targetMap = EntitiesHashMap.quadrantHashMap,
                colliders = CollidersHashMap.quadrantHashMap,
                deltaTime = Time.DeltaTime
            };
            var collisionHandle = collisionForce.Schedule(this);
            collisionHandle.Complete();
        }
    }
}
