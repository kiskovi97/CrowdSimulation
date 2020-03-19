using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct DecidedForce : IComponentData
{
    public float3 force;
}
