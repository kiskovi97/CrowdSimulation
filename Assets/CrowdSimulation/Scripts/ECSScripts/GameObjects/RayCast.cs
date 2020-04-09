using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class RayCast : MonoBehaviour
{

    private Entity Raycast(float3 from, float3 to)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        var collisionWOrld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput input = new RaycastInput()
        {
            Start = from,
            End = to,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 1,
                GroupIndex = 0,
            }
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        if (collisionWOrld.CastRay(input, out hit))
        {
            Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            return hitEntity;
        } else
        {
            return Entity.Null;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = 50f;
            var entity = Raycast(ray.origin, ray.origin + ray.direction * distance);

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Selection>(entity))
            {
                var selection = em.GetComponentData<Selection>(entity);
                selection.Selected = !selection.Selected;
                em.SetComponentData(entity, selection);
            }
        }
    }
}
