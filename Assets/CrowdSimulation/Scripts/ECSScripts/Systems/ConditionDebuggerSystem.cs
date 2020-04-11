using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [UpdateAfter(typeof(EndFrameLocalToParentSystem))]
    class ConditionDebuggerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref ConditionDebugger debugger, ref Parent parent, ref NonUniformScale compositeScale) =>
            {
                if (EntityManager.HasComponent<Condition>(parent.Value))
                {
                    var condition = EntityManager.GetComponentData<Condition>(parent.Value);
                    if (debugger.type == ConditionType.Hunger)
                    {
                        if (condition.hunger > 1f)
                            compositeScale.Value.y = condition.hunger * 0.01f;
                        else
                            compositeScale.Value.y = 0f;
                    }
                    if (debugger.type == ConditionType.LifeLine)
                    {
                        if (condition.lifeLine > 0f)
                            compositeScale.Value.y = condition.lifeLine * 0.01f;
                        else
                            compositeScale.Value.y = 0f;
                    }

                }
            });

            var eqd = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Condition) }
            };
            var query = GetEntityQuery(eqd);
            var entities = query.ToEntityArray(Allocator.TempJob);
            var conditions = query.ToComponentDataArray<Condition>(Allocator.TempJob);
            var destroyable = new NativeList<Entity>(Allocator.TempJob);
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var condition = conditions[i];
                if (condition.lifeLine < 0f)
                {
                    destroyable.Add(entity);
                }
            }
            EntityManager.DestroyEntity(destroyable);
            entities.Dispose();
            conditions.Dispose();
            destroyable.Dispose();
        }
    }
}

