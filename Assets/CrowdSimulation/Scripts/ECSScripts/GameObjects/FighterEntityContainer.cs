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

        public Dictionary<int, List<Entity>> entities = new Dictionary<int, List<Entity>>();
        public Queue<Entity> foreachHelp = new Queue<Entity>();
        public List<int> groupIds = new List<int>();

        private static bool updated = false;

        private void Awake()
        {
            instance = this;
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

                if (instance.groupIds.Count > 1)
                {
                    foreach(var id in instance.groupIds)
                    {
                        if (id != fighter.groupId)
                        {
                            fighter.targerGroupId = id;
                            break;
                        }
                    }
                }

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
