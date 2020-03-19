using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PathForce : IComponentData
{
    public float3 force;
}
