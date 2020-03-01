using Unity.Entities;
using Unity.Mathematics;

public struct GroupCondition : IComponentData
{
    public float3 goalPoint;
    public float goalRadius;
}
