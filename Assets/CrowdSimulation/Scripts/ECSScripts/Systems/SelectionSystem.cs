using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public class SelectionSystem : ComponentSystem
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
            var buffer = endSimulation.CreateCommandBuffer();
            Entities.ForEach((Entity entity, ref Selection selection) => {
                if (!selection.changed) return;
                selection.changed = false;
                if (EntityManager.HasComponent<RenderMesh>(entity))
                {
                    var mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                    mesh.material = selection.Selected ? Materails.Instance.selected : Materails.Instance.notSelected;
                    buffer.SetSharedComponent(entity, mesh);
                }
            });
        }
    }
}