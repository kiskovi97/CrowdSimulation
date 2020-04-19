using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.CrowdSimulation.Scripts.UI
{
    public class LoadingUI : MonoBehaviour
    {
        public Transform loadingCircle;
        public float loading = 1f;

        private Entity entity;

        void Update()
        {
            loadingCircle.localScale = new Vector3(loading, loading, loading);
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (em.HasComponent<Translation>(entity))
            {
                var translate = em.GetComponentData<Translation>(entity);
                var pos = Camera.main.WorldToScreenPoint(translate.Value + new float3(0, 2, 0));
                transform.position = pos;
            } else
            {
                transform.position = new Vector2(0, Camera.main.pixelHeight);
            }
            if (em.HasComponent<SpawnerParameters>(entity))
            {
                var spawn = em.GetComponentData<SpawnerParameters>(entity);
                loading = 1f - spawn.spawnTimer / spawn.spawnTime;
            }
        }

        public void NewPeople()
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
        }
    }
}
