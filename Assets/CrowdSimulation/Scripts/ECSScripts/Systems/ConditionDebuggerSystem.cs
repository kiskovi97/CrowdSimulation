using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [UpdateAfter(typeof(EndFrameLocalToParentSystem))]
    class ConditionDebuggerSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref ConditionDebugger debugger, ref Parent parent, ref NonUniformScale compositeScale) =>
            {
                if (!EntityManager.HasComponent<Condition>(parent.Value)) return;
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
                    var rendererMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                    compositeScale.Value.y = condition.lifeLine * 0.01f;
                    var selectedMaterial = Materails.Instance.rest;
                    if (condition.lifeLine < condition.maxLifeLine)
                    {
                        selectedMaterial = Materails.Instance.healing;
                    }
                    if (condition.hurting > 0f)
                    {
                        selectedMaterial = Materails.Instance.hurting;
                    }
                    if (condition.lifeLine < 30f)
                    {
                        selectedMaterial = Materails.Instance.toLow;
                    }
                    if (rendererMesh.material == selectedMaterial) return;
                    PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                    {
                        material = selectedMaterial,
                        mesh = rendererMesh.mesh
                    });
                }
            });
        }
    }
}

