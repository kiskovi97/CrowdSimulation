using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GroupForce : IComponentData
{
    public float3 force;
}
