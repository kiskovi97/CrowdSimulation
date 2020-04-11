using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class RayCast : MonoBehaviour
    {
        public RectTransform selectionBox;
        private Vector2 startPos;

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
            if (IsPointerOverUIObject())
            {
                selectionBox.gameObject.SetActive(false);
                return;
            }

            if (Input.GetKey(KeyCode.E))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    startPos = Input.mousePosition;
                    selectionBox.gameObject.SetActive(true);
                }

                if (Input.GetMouseButton(0))
                {
                    Vector2 endPos = Input.mousePosition;

                    var dir = endPos - startPos;

                    selectionBox.sizeDelta = math.abs(dir);
                    selectionBox.anchoredPosition = (startPos + endPos) / 2f;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 endPos = Input.mousePosition;
                    selectionBox.gameObject.SetActive(false);
                    SelectSquere();
                }

                return;
            }

            selectionBox.gameObject.SetActive(false);

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

        private void SelectSquere()
        {
            Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
            Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var all = em.GetAllEntities(Unity.Collections.Allocator.Temp);
            foreach (var entity in all)
            {
                if (em.HasComponent<Translation>(entity))
                {
                    var translation = em.GetComponentData<Translation>(entity);
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(translation.Value);

                    if (min.x < screenPos.x && min.y < screenPos.y && max.x > screenPos.x && max.y > screenPos.y)
                    {
                        if (em.HasComponent<Selection>(entity))
                        {
                            var selection = em.GetComponentData<Selection>(entity);
                            selection.Selected = true;
                            em.SetComponentData(entity, selection);
                        }
                    }
                }
            }

        }

        public static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results = results.Where((result) => !result.gameObject.tag.Equals("Ignore")).ToList();
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
}
