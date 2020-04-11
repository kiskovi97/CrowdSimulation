using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;


namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public class NoParentDestroySystem : ComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem endSimulation;

        protected override void OnCreate()
        {
            endSimulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        protected override void OnUpdate()
        {
            var eqd = new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(Parent) },
                All = new ComponentType[] { typeof(NoParentDestroy) }
            };
            var query = GetEntityQuery(eqd);
            var entities = query.ToEntityArray(Allocator.TempJob);

            var buffer = endSimulation.CreateCommandBuffer();
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                DestroyChild(entity, buffer);
                buffer.DestroyEntity(entity);
            }
            entities.Dispose();
        }

        private void DestroyChild(Entity entity, EntityCommandBuffer buffer)
        {
            if (!EntityManager.HasComponent<Child>(entity)) return;
            var children = EntityManager.GetBuffer<Child>(entity);

            NativeArray<Entity> entities = new NativeArray<Entity>(children.Length, Allocator.TempJob);
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                DestroyChild(child.Value, buffer);
                entities[i] = child.Value;
                buffer.DestroyEntity(child.Value);
            }
            entities.Dispose();
        }
    }
}

