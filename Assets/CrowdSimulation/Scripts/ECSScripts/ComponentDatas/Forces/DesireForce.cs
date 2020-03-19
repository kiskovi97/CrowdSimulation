using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct DesireForce : IComponentData
{
    public float3 force;
}
