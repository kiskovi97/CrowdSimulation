﻿using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [UpdateAfter(typeof(FighterSystem))]
    public class AnimatorSystem : ComponentSystem
    {
        public struct AnimationStep
        {
            public float3 position;
            public float3 rotation;
            public float time;
        }

        public struct Animation
        {
            public int startIndex;
            public int endIndex;
        }

        public static NativeArray<Animation> animations;
        public static NativeArray<AnimationStep> animationSteps;

        private EntityQuery pathfindingGroup;

        protected override void OnCreate()
        {
            var pathFindingQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Translation), typeof(Rotation), typeof(AnimatorData) },
            };
            pathfindingGroup = GetEntityQuery(pathFindingQuery);

            base.OnCreate();

            animationSteps = new NativeArray<AnimationStep>(new AnimationStep[] {
            /// RABBIT STEPS
            new AnimationStep()
            {
                position = new float3(0,0,0),
                rotation = new float3(- math.radians(90),0,0), time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = new float3(-0.4f - math.radians(90),0,0),
                time = 0.1f
            },
            new AnimationStep()
            {
                position = new float3(0,0.02f,0),
                rotation = new float3(- math.radians(90),0,0),
                time = 0.5f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = new float3(0.4f - math.radians(90),0,0),
                time = 0.8f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(- math.radians(90),0,0),
                time = 1f
            },

            /// ARM STEPS
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(- math.radians(0),0,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(- math.radians(-90),0,0),
                time = 0.2f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(- math.radians(0),0,0),
                time = 0.4f
            },

             /// BodySteps
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(math.radians(-90),math.PI,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(math.radians(-90),math.PI * 1.9f,0),
                time = 0.1f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(math.radians(-90), math.PI * 2.7f,0),
                time = 0.2f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(math.radians(-90), math.PI * 3f,0),
                time = 0.21f
            },

             /// ArmLong
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(0,-math.PI / 4,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = new float3(0,-math.PI / 4,0),
                time = 0.1f
            },
        }, Allocator.Persistent);

            animations = new NativeArray<Animation>(new Animation[] {
            new Animation() { startIndex = 0, endIndex = 4, },
            new Animation() { startIndex = 5, endIndex = 7, },
            new Animation() { startIndex = 8, endIndex = 11, },
            new Animation() { startIndex = 12, endIndex = 13, },
        }, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            animationSteps.Dispose();
            animations.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
            var deltaTime = Time.DeltaTime;
            var job = new AnimatorJob()
            {
                deltaTime = deltaTime,
                animations = animations,
                steps = animationSteps,
                hashMap = FighterSystem.hashMap,
                random = random,
                AnimatorHandle = GetComponentTypeHandle<AnimatorData>(),
                RotationHandle = GetComponentTypeHandle<Rotation>(),
                TranslationHandle = GetComponentTypeHandle<Translation>(),
            };
            var handle = job.Schedule(pathfindingGroup);
            handle.Complete();
        }
    }
}
