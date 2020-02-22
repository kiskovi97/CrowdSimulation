using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public struct People : IComponentData
{
    public int crowdId;

    public float3 direction;
    public bool debug;
    public float maxSpeed;
    public float4 desire;
}
