using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct Animator : IComponentData
{
    public int animationIndex;
    public float speed;
    public float currentTime;
    public float3 localPos;
    public quaternion localRotation;

    public bool reverseY;
    public int entityReference;
}
