using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Infection : IComponentData
{
    public float infectionTime;
    public float reverseImmunity;

    public static readonly float immunityMultiplyer = 0.1f;
    public static readonly float infectionDistance = 1.5f;
    public static readonly float illTime = 10f;
    public static readonly float infectionChance = 0.8f;
}
