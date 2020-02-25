using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class WalkerObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public float maxSpeed;
    public float3 direction = Vector3.left;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float3 direction = transform.forward;
        dstManager.AddComponentData(entity, new Walker
        {
            direction = direction,
            maxSpeed = maxSpeed,
        });
    }
}
