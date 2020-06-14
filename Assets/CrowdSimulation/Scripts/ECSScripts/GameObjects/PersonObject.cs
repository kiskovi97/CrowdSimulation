using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class PersonObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        private static bool First = true;

        public float3 direction;
        public float maxSpeed;
        private int broId;
        public PathFindingData pathFindingData = new PathFindingData()
        {
            decisionMethod = DecisionMethod.Max,
            avoidMethod = CollisionAvoidanceMethod.No
        };


        CrowdSpawner parent;
        public void ConnectParent(CrowdSpawner parent)
        {
            this.parent = parent;
        }

        private GroupCondition condition = new GroupCondition { };
        private bool conditionAdded = false;

        public void ChangeGroup(GroupCondition condition, int broId, PathFindingData pathFindingData)
        {
            this.condition = condition;
            this.broId = broId;
            this.pathFindingData = pathFindingData;
            conditionAdded = true;
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (conditionAdded)
            {
                dstManager.AddComponentData(entity, condition);
            }
            dstManager.AddComponentData(entity, new Walker
            {
                direction = direction,
                maxSpeed = maxSpeed,
                broId = broId,
            });
            dstManager.AddComponentData(entity, pathFindingData);

            if (parent != null)
                parent.AddEntity(entity);

            if (First && pathFindingData.avoidMethod == CollisionAvoidanceMethod.Probability)
            {
                ProbabilitySystem.selected = entity;
                First = false;
            }
        }
    }
}
