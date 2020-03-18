using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Infection : IComponentData
{
    public float infectionTime;
    public float reverseImmunity;
}
