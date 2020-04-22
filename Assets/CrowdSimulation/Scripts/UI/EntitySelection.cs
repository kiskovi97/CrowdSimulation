using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.CrowdSimulation.Scripts.UI
{
    class EntitySelection : MonoBehaviour
    {
        public RectTransform selectionBox;
        public LoadingUI loadingIcon;

        public static EntitySelection instance;

        private static RectTransform SelectionBox;
        private static Vector2 startPos;

        public void OnEscape()
        {
            Application.Quit();
        }

        private void Start()
        {
            instance = this;
            SelectionBox = selectionBox;
        }

        public static void OnSelectionBegin()
        {
            startPos = Mouse.current.position.ReadValue();
            SelectionBox.gameObject.SetActive(true);
        }

        public static void OnSelectionEnd()
        {
            SelectionBox.gameObject.SetActive(false);
            Vector2 min = SelectionBox.anchoredPosition - (SelectionBox.sizeDelta / 2);
            Vector2 max = SelectionBox.anchoredPosition + (SelectionBox.sizeDelta / 2);
            SelectionBox.sizeDelta = Vector2.zero;

            SelectSquere(min, max);
        }

        public static void SetSelectionBoxFromPosition(Vector2 endPos)
        {
            var dir = endPos - startPos;
            SelectionBox.sizeDelta = math.abs(dir);
            SelectionBox.anchoredPosition = (startPos + endPos) / 2f;
        }

        public static void SelectEntity(Entity entity)
        {
            instance.loadingIcon.SetEntity(entity);
            if (entity == Entity.Null) return;

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Selectable>(entity))
            {
                var selection = em.GetComponentData<Selectable>(entity);
                FighterEntityContainer.SetSelect(!selection.Selected, entity, em);
            }
        }

        public static void SelectedSetGoalPoint(float3 goalPoint)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var all = em.GetAllEntities(Unity.Collections.Allocator.Temp);
            var count = 0;
            foreach (var entity in all)
            {
                if (!em.HasComponent<Selectable>(entity))
                {
                    continue;
                }
                var selection = em.GetComponentData<Selectable>(entity);
                if (!selection.Selected)
                {
                    continue;
                }
                if (!em.HasComponent<Fighter>(entity))
                {
                    continue;
                }
                count++;
                var radius = math.sqrt(count / math.PI);
                var fighter = em.GetComponentData<Fighter>(entity);
                fighter.goalPos = goalPoint;
                fighter.goalRadius = radius;
                em.SetComponentData(entity, fighter);
            }
        }

        private static void SelectSquere(Vector2 min, Vector2 max)
        {
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
                        if (em.HasComponent<Selectable>(entity))
                        {
                            FighterEntityContainer.SetSelect(true, entity, em);
                        }
                    }
                }
            }
        }
    }
}
