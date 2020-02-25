using Unity.Entities;
using Unity.Mathematics;

public struct CollisionForce : IComponentData
{
    public float3 force;
}
