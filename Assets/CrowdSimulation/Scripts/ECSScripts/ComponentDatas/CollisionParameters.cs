
using Unity.Entities;

public struct CollisionParameters : IComponentData
{
    public float innerRadius;
    public float outerRadius;
}
