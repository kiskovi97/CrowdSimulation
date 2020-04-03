using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using System;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Animator : IComponentData
{
    public int animationIndex;
    public float speed;
    public float currentTime;
    public float3 localPos;
    public quaternion localRotation;
}
