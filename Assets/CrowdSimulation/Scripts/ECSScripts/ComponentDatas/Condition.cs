using Unity.Entities;

[GenerateAuthoringComponent]
public struct Condition : IComponentData
{
    public float lifeLine;
    public float hunger;
    public float thirst;
}
