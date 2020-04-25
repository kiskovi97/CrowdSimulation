using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class SpawnerObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int groupId;
        public float spawnTime;
        public int maxEntity;
        public int level = 1;

        public Transform offset;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SpawnerParameters
            {
                level = level,
                groupId = groupId,
                simple = new OneSpawnParameter()
                {
                    spawnTime = spawnTime,
                    spawnTimer = 0,
                    maxEntity = maxEntity,
                    currentEntity = 0,
                },
                master = new OneSpawnParameter()
                {
                    spawnTime = spawnTime * 2,
                    spawnTimer = 0,
                    maxEntity = maxEntity,
                    currentEntity = 0,
                },
                offset = transform.TransformDirection(offset.localPosition),
            });

            ShortestPathSystem.AddGoalPoint(offset.position);
        }
    }
}
