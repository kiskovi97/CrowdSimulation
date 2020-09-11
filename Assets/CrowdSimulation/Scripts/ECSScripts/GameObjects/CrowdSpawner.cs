using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using TMPro;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class CrowdSpawner : MonoBehaviour
    {
        public GameObject entityObject;
        public Transform goalPoint;
        [SerializeField]
        public CollisionAvoidanceMethod CollisionAvoidanceMethod;
        [SerializeField]
        public PathFindingMethod PathFindingMethod = PathFindingMethod.No;
        [SerializeField]
        public DecisionMethod decisionMethod;
        public TextMeshProUGUI info;
        public Material color;

        public Vector3 goal { get => goalPoint != null ? goalPoint.position : Vector3.zero; }

        public float goalRadius;

        public static int Id = 0;

        public int sizeX = 5;
        public int sizeZ = 5;

        public float distance = 1f;
        public GroupFormation formation = GroupFormation.Circel;
        public bool fill = true;

        private List<Entity> entities = new List<Entity>();

        public (int infected, int immune) GetEntities(EntityManager em)
        {
            var infected = 0;
            var immune = 0;
            foreach (var entity in entities)
            {
                var infection = em.GetComponentData<Infection>(entity);
                if (infection.infectionTime > 0f)
                {
                    infected++;
                }
                if (infection.reverseImmunity < 1f)
                {
                    immune++;
                }
            }
            return (infected, immune);
        }

        public float EntitiesDistanceFromGoal()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var avrgDistance = 0f;
            foreach (var entity in entities)
            {
                var data = em.GetComponentData<Translation>(entity);
                var pos = data.Value;
                var distance = (new Vector3(pos.x, pos.y, pos.z) - goal).magnitude;
                avrgDistance += math.max(0f, (distance - goalRadius) / (float)entities.Count);
            }
            return avrgDistance;
        }

        internal bool HasEnties()
        {
            return entities != null && entities.Count > 0;
        }

        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }

        // Start is called before the first frame update
        void Start()
        {
            ShortestPathSystem.AddGoalPoint(goal);
            var cond = new GroupCondition() { goalPoint = goal, goalRadius = goalRadius, formation = formation, fill = fill };
            Id++;
            //if (Id >= Map.MaxGroup)
            //{
            //    Id = 1;
            //}

            Map.FullGroup = Id + 1;
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeZ; j++)
                {
                    var position = new Vector3((i - sizeX / 2) * distance, 0, (j - sizeZ / 2) * distance);
                    var obj = Instantiate(entityObject, transform);
                    obj.transform.localPosition = position;
                    var renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null && color != null)
                    {
                        renderer.material = color;
                    }
                    var person = obj.GetComponent<PersonObject>();
                    if (person != null)
                    {
                        person.ChangeGroup(cond, Id, new PathFindingData() { avoidMethod = CollisionAvoidanceMethod, decisionMethod = decisionMethod, pathFindingMethod = PathFindingMethod });
                        person.ConnectParent(this);
                    }
                }
            }
        }

        private void Update()
        {
            if (info != null)
            {
                info.text = "PathFinding: " + CollisionAvoidanceMethod.ToString() + ", Decision: " + decisionMethod.ToString();
                info.color = color.color;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                NewGoal();
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                NewGoal2();
            }
        }

        private void NewGoal()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var entity in entities)
            {
                var data = em.GetComponentData<GroupCondition>(entity);
                data.goalPoint = new float3(0, 0, 0);
                data.goalRadius = 10f;
                em.SetComponentData(entity, data);
            }
        }

        private void NewGoal2()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            foreach (var entity in entities)
            {
                var data = em.GetComponentData<GroupCondition>(entity);
                data.goalPoint *= -1;
                em.SetComponentData(entity, data);
            }
        }
    }
}
