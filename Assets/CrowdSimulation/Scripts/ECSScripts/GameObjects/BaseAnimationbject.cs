using UnityEngine;
using System.Collections;
using Unity.Entities;

public class BaseAnimationbject : MonoBehaviour, IConvertGameObjectToEntity
{
    public int AnimationIndex = 1;
    public bool reverseY = false;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Animator()
        {
            animationIndex = AnimationIndex,
            speed = 1f,
            currentTime = UnityEngine.Random.value,
            localPos = transform.localPosition,
            localRotation = transform.rotation,
            reverseY = reverseY,
        });
    }
}
