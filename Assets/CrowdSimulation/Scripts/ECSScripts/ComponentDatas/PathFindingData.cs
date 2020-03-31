using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
[Serializable]
public struct PathFindingData : IComponentData
{
    public PathFindingMethod pathFindingMethod;
    public DecisionMethod decisionMethod;
}

public enum PathFindingMethod
{
    DensityGrid,
    Forces,
    No
}

public enum DecisionMethod
{
    Plus,
    Max,
    Min
}
