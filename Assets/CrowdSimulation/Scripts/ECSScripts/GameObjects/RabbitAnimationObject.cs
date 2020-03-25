using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RabbitAnimationObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public struct AnimationStep
    {
        public float3 position;
        public quaternion rotation;
        public float time;
    }

    public static NativeArray<AnimationStep> jumping;

    void Awake()
    {
        if (jumping.Length == 0)
            jumping = new NativeArray<AnimationStep>(new AnimationStep[] {
            new AnimationStep()
            {
                position = new float3(0,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = quaternion.EulerXYZ(-0.4f - math.radians(90),0,0),
                time = 0.1f
            },
            new AnimationStep()
            {
                position = new float3(0,0.02f,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 0.5f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = quaternion.EulerXYZ(0.4f - math.radians(90),0,0),
                time = 0.8f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 1f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 1.2f
            }
        }, Allocator.Persistent);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Animator()
        {
            currentTime = UnityEngine.Random.value,
            localPos = transform.localPosition,
            localRotation = transform.rotation,
        });
    }
}
