using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct ConditionDebugger : IComponentData
{
    public ConditionType type;
}

public enum ConditionType
{
    Hunger,
    LifeLine
}
