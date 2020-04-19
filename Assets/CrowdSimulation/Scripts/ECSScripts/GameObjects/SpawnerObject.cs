using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class SpawnerObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int groupId;
        public float spawnTime;
        public int maxEntity;

        public Transform offset;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SpawnerParameters
            {
                groupId = groupId,
                spawnTime = spawnTime,
                spawnTimer = 0,
                maxEntity = maxEntity,
                currentEntity = 0,
                offset = transform.TransformDirection(offset.localPosition),
            });
        }
    }
}
