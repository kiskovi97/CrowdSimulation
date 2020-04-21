using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using System;
using Unity.Transforms;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class FighterCrowdSpawner : MonoBehaviour
    {
        public GameObject[] entityObjects;
        public FighterCrowdSpawner targetCrowd;
        [SerializeField]
        public PathFindingData data;
        public Transform offsetPoint;

        public static int Id = 0;

        [NonSerialized]
        public int myId;

        public int sizeX = 5;
        public int sizeZ = 5;

        public float distance = 1f;

        private List<Entity> entities = new List<Entity>();
        private List<Entity> typeMaster = new List<Entity>();
        private List<Entity> typeSimple = new List<Entity>();


        public void ClearAll()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var entity in entities)
            {
                if (!em.Exists(entity)) continue;
                SetSelect(false, entity, em);
            }
        }

        public void SelectAllMaster()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            bool allSelected = true;
            foreach (var entity in typeMaster)
            {
                if (!em.Exists(entity)) continue;
                var selection = em.GetComponentData<Selection>(entity);
                allSelected &= selection.Selected;
                SetSelect(true, entity, em);
            }
            if (allSelected)
            {
                foreach (var entity in typeMaster)
                {
                    if (!em.Exists(entity)) continue;
                    SetSelect(false, entity, em);
                }
            }
        }

        public void SelectAllSimple()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            bool allSelected = true;
            foreach (var entity in typeSimple)
            {
                if (!em.Exists(entity)) continue;
                var selection = em.GetComponentData<Selection>(entity);
                allSelected &= selection.Selected;
                SetSelect(true, entity, em);
            }
            if (allSelected)
            {
                foreach (var entity in typeSimple)
                {
                    if (!em.Exists(entity)) continue;
                    SetSelect(false, entity, em);
                }
            }
        }

        public void AddEntity(Entity entity, AttackType type)
        {
            entities.Add(entity);
            if (type == AttackType.All)
            {
                typeMaster.Add(entity);
            }
            if (type == AttackType.One)
            {
                typeSimple.Add(entity);
            }
        }

        private void Awake()
        {
            Id++;
            if (Id >= Map.MaxGroup)
            {
                Id = 1;
            }
            myId = Id;
        }

        // Start is called before the first frame update
        void Start()
        {
            var area = sizeX * sizeZ * distance * distance;
            var offset = offsetPoint == null ? Vector3.zero : offsetPoint.position - transform.position;
            var radius = Mathf.Sqrt(area / Mathf.PI);
            var fighterComp = new Fighter()
            {
                goalPos = transform.position + offset,
                goalRadius = radius,
                groupId = myId,

                targetId = -1,
                state = FightState.Standing,
            };
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeZ; j++)
                {
                    var position = new Vector3((i - sizeX / 2) * distance, 0, (j - sizeZ / 2) * distance) + offset;
                    var index = (int)(UnityEngine.Random.value * entityObjects.Length);
                    var obj = Instantiate(entityObjects[index], transform);
                    obj.transform.localPosition = position;
                    var fighter = obj.GetComponent<FighterObject>();
                    if (fighter != null)
                    {
                        fighter.ChangeGroup(myId, data, fighterComp);
                        fighter.ConnectParent(this);
                    }
                }
            }
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

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ClearAll();
            }

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            entities = entities.Where((entity) => em.Exists(entity)).ToList();
            typeMaster = typeMaster.Where((entity) => em.Exists(entity)).ToList();
            typeSimple = typeSimple.Where((entity) => em.Exists(entity)).ToList();

            SetCamera();
        }

        private void SetCamera()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var pos = float3.zero;
            int db = 0;
            foreach (var entity in entities)
            {
                if (!em.Exists(entity)) continue;
                var data = em.GetComponentData<Translation>(entity);
                pos += data.Value;
                db++;
            }
            if (db > 0)
            {
                pos /= db;
                targetCrowd.SetTargetPosition(pos);
            }
        }

        private void SetTargetPosition(float3 pos)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var area = entities.Count * distance * distance;
            var radius = Mathf.Sqrt(area / Mathf.PI);
            foreach (var entity in entities)
            {
                if (!em.Exists(entity)) continue;
                var data = em.GetComponentData<Fighter>(entity);
                data.goalPos = pos;
                data.goalRadius = radius;
                em.SetComponentData(entity, data);
            }
        }

        private void ChangeState(bool fight)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var entity in entities)
            {
                if (!em.Exists(entity)) continue;
                var data = em.GetComponentData<Fighter>(entity);
                data.state = fight ? FightState.GoToFight : FightState.Standing;
                em.SetComponentData(entity, data);
            }
        }

        private void SetSelect(bool select, Entity entity, EntityManager em)
        {
            em.SetComponentData(entity, new Selection() { Selected = select, changed = true});
        }

    }
}
