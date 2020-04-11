
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Entities;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects
{
    public class BaseAnimationbject : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int AnimationIndex = 1;
        public bool reverseY = false;
        public int entityReference;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AnimatorData()
            {
                animationIndex = AnimationIndex,
                speed = 1f,
                currentTime = Random.value,
                localPos = transform.localPosition,
                localRotation = transform.rotation,
                reverseY = reverseY,
                entityReference = entityReference,
            });
        }
    }
}
