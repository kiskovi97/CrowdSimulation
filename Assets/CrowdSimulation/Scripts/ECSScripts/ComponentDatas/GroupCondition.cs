using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GroupCondition : IComponentData
{
    public float3 goalPoint;
    public float goalRadius;
}
