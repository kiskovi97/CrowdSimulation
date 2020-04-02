using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Fighter : IComponentData
{
    public int targetId;
    public float3 targetPos;
    public float3 restPos;
    public float restRadius;
    public bool fight;
}
