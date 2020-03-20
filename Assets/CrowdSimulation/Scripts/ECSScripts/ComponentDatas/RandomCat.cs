using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct RandomCat : IComponentData
{
    public float random;
}
