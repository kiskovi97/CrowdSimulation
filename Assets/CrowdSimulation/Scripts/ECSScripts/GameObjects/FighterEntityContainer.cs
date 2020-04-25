using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    [RequireComponent(typeof(PlayerInput))]
    public class FighterEntityContainer : MonoBehaviour
    {
        public static FighterEntityContainer instance;

        private readonly Dictionary<int, List<Entity>> entities = new Dictionary<int, List<Entity>>();
        private readonly Queue<Entity> foreachHelp = new Queue<Entity>();
        private readonly List<int> groupIds = new List<int>();

        private static bool updated = false;

        private void Awake()
        {
            instance = this;
        }

        public void OnAttack()
        {
            ChangeState();
        }

        public void ClearAll()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var list in entities.Values)
            {
                foreach (var entity in list)
                {
                    if (!em.Exists(entity)) continue;
                    if (!em.HasComponent<Selectable>(entity)) continue;
                    SetSelect(false, entity, em);
                }
            }
        }

        public void SetAll(int groupId)
        {
            if (!entities.ContainsKey(groupId)) return;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            bool all = true;
            foreach(var entity in entities[groupId])
            {
                if (!em.Exists(entity)) continue;
                if (!em.HasComponent<Selectable>(entity)) continue;
                var selection = em.GetComponentData<Selectable>(entity);
                all &= selection.Selected;
                SetSelect(true, entity, em);
            }
            if (all)
            {
                foreach (var entity in entities[groupId])
                {
                    if (!em.Exists(entity)) continue;
                    if (!em.HasComponent<Selectable>(entity)) continue;
                    SetSelect(false, entity, em);
                }
            }
        }

        public static void AddEntity(Entity entity)
        {
            if (instance != null)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                var fighter = em.GetComponentData<Fighter>(entity);

                if (!instance.entities.ContainsKey(fighter.groupId))
                {
                    instance.entities.Add(fighter.groupId, new List<Entity>());
                    instance.groupIds.Add(fighter.groupId);
                }
                instance.entities[fighter.groupId].Add(entity);
                instance.foreachHelp.Enqueue(entity);
            }

            updated = true;
        }

        private void Update()
        {
            if (updated)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                foreach (var list in entities.Values)
                {
                    var area = list.Count;
                    var radius = Mathf.Sqrt(area / Mathf.PI);
                    foreach (var ent in list)
                    {
                        if (!em.Exists(ent)) continue;
                        if (!em.HasComponent<Fighter>(ent)) continue;
                        var fighter = em.GetComponentData<Fighter>(ent);
                        fighter.goalRadius = radius;
                        em.SetComponentData(ent, fighter);
                    }
                }
                updated = false;
            }

            while(foreachHelp.Count > 0)
            {
                var ent = foreachHelp.Dequeue();
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (!em.HasComponent<Child>(ent))
                {
                    foreachHelp.Enqueue(ent);
                    return;
                }
                ForeachChildren(ent, ent.Index, em);
            }
        }

        private static void ForeachChildren(Entity entity, int kod, EntityManager manager)
        {
            if (manager.HasComponent<AnimatorData>(entity))
            {
                var animator = manager.GetComponentData<AnimatorData>(entity);
                animator.entityReference = kod;
                manager.SetComponentData(entity, animator);
            }

            if (!manager.HasComponent<Child>(entity))
            {
                return;
            }
            var children = manager.GetBuffer<Child>(entity);
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                ForeachChildren(child.Value, kod, manager);
            }
        }

        private void ChangeState()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var list in entities.Values)
            {
                foreach(var entity in list)
                {
                    if (!em.Exists(entity)) continue;
                    var data = em.GetComponentData<Fighter>(entity);
                    data.goalPos = new float3(0,2,0);
                    em.SetComponentData(entity, data);
                }
            }

            ShortestPathSystem.AddGoalPoint(new float3(0, 2, 0));
        }

        public static void SetSelect(bool select, Entity entity, EntityManager em)
        {
            em.SetComponentData(entity, new Selectable() { Selected = select, changed = true });
        }
    }
}
