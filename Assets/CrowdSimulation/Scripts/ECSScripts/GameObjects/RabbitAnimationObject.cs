using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Entities;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class RabbitAnimationObject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AnimatorData()
            {
                animationIndex = 0,
                speed = 1f,
                currentTime = Random.value,
                localPos = transform.localPosition,
                localRotation = transform.rotation,
            });
        }
    }
}
