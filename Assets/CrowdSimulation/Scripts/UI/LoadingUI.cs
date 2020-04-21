using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.CrowdSimulation.Scripts.UI
{
    public class LoadingUI : MonoBehaviour
    {
        public Image[] loadingCircleImg;
        public Image[] colorables;
        public TextMeshProUGUI[] text;
        public float[] loading;

        public Color[] colors;

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

            for ( int id = 0; id < loading.Length; id++)
            {
                loadingCircleImg[id].fillAmount = loading[id];
                if (em.HasComponent<SpawnerParameters>(entity))
                {
                    var spawn = em.GetComponentData<SpawnerParameters>(entity);
                    loading[id] = 1f - spawn.spawnTimer / spawn.spawnTime;
                    text[id].text = $"{spawn.currentEntity} / {spawn.maxEntity}";
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
                foreach (var image in colorables)
                {
                    image.color = colors[spawn.groupId];
                }
            }
        }
    }
}
