using Unity.Entities;
using Unity.Mathematics;

public struct Walker : IComponentData
{
    public float3 direction;
    public float maxSpeed;
}
