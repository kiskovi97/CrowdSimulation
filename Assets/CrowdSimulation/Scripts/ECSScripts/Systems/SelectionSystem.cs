using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

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
        var query = GetEntityQuery((typeof(Selection)));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var selections = query.ToComponentDataArray<Selection>(Unity.Collections.Allocator.TempJob);

        for(int i=0; i<entities.Length; i++)
        {
            var entity = entities[i];
            var selection = selections[i];
            var mesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
            mesh.material = selection.Selected ? Materails.Instance.selected : Materails.Instance.notSelected;
            EntityManager.SetSharedComponentData(entity, mesh);
        }
        entities.Dispose();
        selections.Dispose();
    }
}