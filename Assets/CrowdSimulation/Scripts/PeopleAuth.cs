using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PeopleAuth : MonoBehaviour, IConvertGameObjectToEntity
{
    public float maxSpeed;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float3 direction = transform.forward;
        dstManager.AddComponentData(entity, new People
        {
            direction = direction,
            debug = false,
            maxSpeed = maxSpeed,
            desire = direction,
        });
    }
}
