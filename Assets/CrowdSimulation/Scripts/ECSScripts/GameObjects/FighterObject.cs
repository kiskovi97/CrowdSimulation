using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class FighterObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float maxSpeed = 2f;
        public float attackRadius = 2f;
        private int broId;
        public float attackStrength = 2f;
        public AttackType attackType = AttackType.One;

        private PathFindingData pathFindingData = new PathFindingData()
        {
            decisionMethod = DecisionMethod.Max,
            pathFindingMethod = PathFindingMethod.No
        };
        private Fighter fighter = new Fighter()
        {
            targerGroupId = 0
        };

        FighterCrowdSpawner parent;
        public void ConnectParent(FighterCrowdSpawner parent)
        {
            this.parent = parent;
        }

        private bool changedGroup = false;

        public void ChangeGroup(int broId, PathFindingData pathFindingData, Fighter fighter)
        {
            this.broId = broId;
            this.pathFindingData = pathFindingData;
            this.fighter = fighter;
            this.fighter.attackRadius = attackRadius;
            this.fighter.attack = attackType;
            this.fighter.attackStrength = attackStrength;
            changedGroup = true;
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            if (changedGroup)
            {
                this.fighter.Id = entity.Index;
                dstManager.AddComponentData(entity, fighter);
                dstManager.AddComponentData(entity, pathFindingData);
            }
            dstManager.AddComponentData(entity, new Walker
            {
                direction = float3.zero,
                maxSpeed = maxSpeed,
                broId = broId,
            });

            if (parent != null)
                parent.AddEntity(entity, fighter.attack);

            ForeachChildren(entity, entity.Index, dstManager);
            ForeachRealChildren(transform, entity.Index);
        }

        private void ForeachRealChildren(Transform transform, int kod)
        {
            foreach (Transform child in transform)
            {
                var animator = child.gameObject.GetComponent<BaseAnimationbject>();
                if (animator != null)
                {
                    animator.entityReference = kod;
                }
                ForeachRealChildren(child, kod);
            }
        }

        private void ForeachChildren(Entity entity, int kod, EntityManager manager)
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
