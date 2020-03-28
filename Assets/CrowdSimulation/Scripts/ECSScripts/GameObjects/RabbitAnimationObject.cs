using Unity.Entities;
using UnityEngine;

public class RabbitAnimationObject : MonoBehaviour, IConvertGameObjectToEntity
{
   
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Animator()
        {
            currentTime = UnityEngine.Random.value,
            localPos = transform.localPosition,
            localRotation = transform.rotation,
        });
    }
}
