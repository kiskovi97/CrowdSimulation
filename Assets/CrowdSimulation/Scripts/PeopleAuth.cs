using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PeopleAuth : MonoBehaviour, IConvertGameObjectToEntity
{
    public float maxSpeed;
    public int crowdId;

    public bool desire = true;
    public float3 goal = new float3();

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float3 direction = transform.forward;
        if (desire)
        {
            dstManager.AddComponentData(entity, new People
            {
                crowdId = crowdId,
                direction = direction,
                debug = false,
                maxSpeed = maxSpeed,
                desire = new float4(direction, 0),
            });
        } else
        {
            dstManager.AddComponentData(entity, new People
            {
                crowdId = crowdId,
                direction = direction,
                debug = false,
                maxSpeed = maxSpeed,
                desire = new float4(goal, 1),
            });
        }
    }
}
