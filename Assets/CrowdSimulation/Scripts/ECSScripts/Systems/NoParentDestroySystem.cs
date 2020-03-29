using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class NoParentDestroySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var eqd = new EntityQueryDesc
        {
            None = new ComponentType[] { typeof(Parent) },
            All = new ComponentType[] { typeof(NoParentDestroy) }
        };
        var query = GetEntityQuery(eqd);
        var entities = query.ToEntityArray(Allocator.TempJob);
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            DestroyChild(entity);
            Debug.Log(EntityManager.GetName(entity));
        }
        entities.Dispose();
        EntityManager.DestroyEntity(query);
    }

    private void DestroyChild(Entity entity)
    {
        if (!EntityManager.HasComponent<Child>(entity)) return;
        var children = EntityManager.GetBuffer<Child>(entity);

        NativeArray<Entity> entities = new NativeArray<Entity>(children.Length, Allocator.TempJob);
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            DestroyChild(child.Value);
            entities[i] = child.Value;
            Debug.Log(EntityManager.GetName(child.Value));
        }
        EntityManager.DestroyEntity(entities);
        entities.Dispose();
    }
}

