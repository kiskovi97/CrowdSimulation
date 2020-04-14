using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class FighterEntityContainer : MonoBehaviour
    {
        public static FighterEntityContainer instance;

        private Dictionary<int, List<Entity>> entities = new Dictionary<int, List<Entity>>();
        private Queue<Entity> foreachHelp = new Queue<Entity>();
        private List<int> groupIds = new List<int>();

        private static bool updated = false;

        private void Awake()
        {
            instance = this;
        }

        public void ClearAll()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var list in entities.Values)
            {
                foreach (var entity in list)
                {
                    if (!em.Exists(entity)) continue;
                    if (!em.HasComponent<Selection>(entity)) continue;
                    var data = em.GetComponentData<Selection>(entity);
                    data.Selected = false;
                    em.SetComponentData(entity, data);
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
                if (!em.HasComponent<Selection>(entity)) continue;
                var selection = em.GetComponentData<Selection>(entity);
                all &= selection.Selected;
                selection.Selected = true;
                em.SetComponentData(entity, selection);
            }
            if (all)
            {
                foreach (var entity in entities[groupId])
                {
                    if (!em.Exists(entity)) continue;
                    if (!em.HasComponent<Selection>(entity)) continue;
                    var selection = em.GetComponentData<Selection>(entity);
                    selection.Selected = false;
                    em.SetComponentData(entity, selection);
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

                var area = instance.entities[fighter.groupId].Count;
                var radius = Mathf.Sqrt(area / Mathf.PI);
                fighter.restRadius = radius;

                instance.entities[fighter.groupId].Add(entity);
                em.SetComponentData(entity, fighter);
                instance.foreachHelp.Enqueue(entity);
            }

            updated = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeState(true);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ChangeState(false);
            }

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
                        fighter.restRadius = radius;
                        em.SetComponentData(ent, fighter);
                    }
                }
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

        private void ChangeState(bool fight)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var list in entities.Values)
            {
                foreach(var entity in list)
                {
                    if (!em.Exists(entity)) continue;
                    var data = em.GetComponentData<Fighter>(entity);
                    data.state = fight ? FightState.GoToFight : FightState.Rest;
                    em.SetComponentData(entity, data);
                }
            }
        }
    }
}
