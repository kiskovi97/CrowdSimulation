using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Walker : IComponentData
{
    public int broId;
    public float3 direction;
    public float maxSpeed;
}
