using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CrowdSimulation.Scripts.UI
{
    public class LoadingUI : MonoBehaviour
    {
        private readonly List<LoadingFighter> fighters = new List<LoadingFighter>();
        public RectTransform parentLayout;
        public LoadingFighter prefab;
        public RectTransform buble;

        public Color[] colors;
        public Sprite[] icons;

        private Entity entity;
        private float originalHeight;

        void Start()
        {
            originalHeight = buble.sizeDelta.y;
        }

        void LateUpdate()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (em.HasComponent<Translation>(entity) && em.HasComponent<SpawnerParameters>(entity))
            {
                var translate = em.GetComponentData<Translation>(entity);
                var pos = Camera.main.WorldToScreenPoint(translate.Value + new float3(0, 2, 0));
                transform.position = pos;

                var spawn = em.GetComponentData<SpawnerParameters>(entity);
                if (spawn.level > 0 && fighters.Count > 0)
                {
                    var loading = 1f - spawn.simple.spawnTimer / spawn.simple.spawnTime;
                    fighters[0].SetLoading(loading, spawn.simple.currentEntity, spawn.simple.maxEntity);
                }
                if (spawn.level > 1 && fighters.Count > 1)
                {
                    var loading = 1f - spawn.master.spawnTimer / spawn.master.spawnTime;
                    fighters[1].SetLoading(loading, spawn.master.currentEntity, spawn.master.maxEntity);
                }
            }
            else
            {
                transform.position = new Vector2(0, Camera.main.pixelHeight);
            }

        }

        public void NewPeople(int id)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<SpawnerParameters>(entity))
            {
                var spawn = em.GetComponentData<SpawnerParameters>(entity);
                if (id == 0) spawn.simple.maxEntity++;
                if (id == 1) spawn.master.maxEntity++;
                em.SetComponentData(entity, spawn);
            }
        }

        internal void SetEntity(Entity entity)
        {
            this.entity = entity;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<SpawnerParameters>(entity))
            {
                var spawn = em.GetComponentData<SpawnerParameters>(entity);
                Clear();
                for (var i = 0; i< spawn.level; i++)
                {
                    AddFighter(spawn.groupId);
                }
                Vector2 size = new Vector2(originalHeight * spawn.level, originalHeight );
                buble.sizeDelta = size;
            }
            else
            {
                transform.position = new Vector2(0, Camera.main.pixelHeight);
            }
        }

        private void Clear()
        {
            foreach (var fighter in fighters)
            {
                Destroy(fighter.gameObject);
            }
            fighters.Clear();
        }

        private void AddFighter(int groupId)
        {
            var obj = Instantiate(prefab.gameObject, parentLayout);
            var fighter = obj.GetComponent<LoadingFighter>();
            var button = obj.GetComponent<Button>();
            var id = fighters.Count;

            button.onClick.AddListener(() => NewPeople(id));
            fighter.SetImage(icons[id]);
            fighter.SetColor(colors[groupId]);
            fighters.Add(fighter);
        }
    }
}
