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

            if (updated)
            {
                var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                foreach (var list in entities.Values)
                {
                    var area = list.Count;
                    var radius = Mathf.Sqrt(area / Mathf.PI);
                    foreach (var ent in list)
                    {
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
    }
}
