using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;

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
        }
        else
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
        if (IsPointerOverUIObject()) return;
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = 50f;
            var entity = Raycast(ray.origin, ray.origin + ray.direction * distance);
            if (entity == Entity.Null)
            {
                MonoBehaviourRayCast(ray);
                return;
            }
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Selection>(entity))
            {
                var selection = em.GetComponentData<Selection>(entity);
                selection.Selected = !selection.Selected;
                em.SetComponentData(entity, selection);
            }
        }
    }
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void MonoBehaviourRayCast(UnityEngine.Ray ray)
    {
        if (GUIUtility.hotControl != 0) return;
        if (Physics.Raycast(ray, out UnityEngine.RaycastHit hit, 100f))
        {
            var goalPoint = hit.point;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var all = em.GetAllEntities(Unity.Collections.Allocator.Temp);
            foreach (var entity in all)
            {
                if (em.HasComponent<Selection>(entity))
                {
                    var selection = em.GetComponentData<Selection>(entity);
                    if (selection.Selected)
                    {
                        if (em.HasComponent<Fighter>(entity))
                        {
                            var fighter = em.GetComponentData<Fighter>(entity);
                            fighter.restPos = goalPoint;
                            em.SetComponentData(entity, fighter);
                        }
                    }
                }
            }
        }
    }
}
