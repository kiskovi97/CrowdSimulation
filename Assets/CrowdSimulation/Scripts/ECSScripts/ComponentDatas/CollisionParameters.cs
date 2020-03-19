
using Unity.Entities;

[GenerateAuthoringComponent]
public struct CollisionParameters : IComponentData
{
    public float innerRadius;
    public float outerRadius;
}
