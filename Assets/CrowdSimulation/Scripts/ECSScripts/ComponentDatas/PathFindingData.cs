using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PathFindingData : IComponentData
{
    public PathFindingMethod method;
}

public enum PathFindingMethod
{
    DensityGrid,
    Forces,
    No
}
