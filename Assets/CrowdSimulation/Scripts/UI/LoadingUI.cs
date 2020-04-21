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

        public Color[] colors;
        public Sprite[] icons;

        private Entity entity;


        void Update()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (em.HasComponent<Translation>(entity))
            {
                var translate = em.GetComponentData<Translation>(entity);
                var pos = Camera.main.WorldToScreenPoint(translate.Value + new float3(0, 2, 0));
                transform.position = pos;
            }
            else
            {
                transform.position = new Vector2(0, Camera.main.pixelHeight);
            }

            for ( int id = 0; id < fighters.Count; id++)
            {
                if (em.HasComponent<SpawnerParameters>(entity))
                {
                    var spawn = em.GetComponentData<SpawnerParameters>(entity);
                    var loading = 1f - spawn.spawnTimer / spawn.spawnTime;
                    fighters[id].SetLoading(loading,spawn.currentEntity,spawn.maxEntity);
                }
            }
        }

        public void NewPeople(int id)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<SpawnerParameters>(entity))
            {
                var spawn = em.GetComponentData<SpawnerParameters>(entity);
                spawn.maxEntity++;
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
                //
                AddFighter(spawn.groupId);
                AddFighter(spawn.groupId);
                AddFighter(spawn.groupId);
            }
        }

        private void Clear()
        {
            foreach(var fighter in fighters)
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
