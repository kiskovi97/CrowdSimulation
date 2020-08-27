using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static Map;

//CollisionSystem

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(CollisionSystem))]
    class DensitySystem : ComponentSystem
    {
        private static int batchSize = 64;

        public static NativeArray<float> densityMatrix;

        public static NativeArray<float> collidersDensity;

        public static float3 Right => new float3((1f / Map.density), 0, 0);
        public static float3 Up => new float3(0, 0, (1f / Map.density));

        private EntityQuery entityQuery;
        protected override void OnCreate()
        {
            var tempQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Walker), typeof(Rotation), typeof(Translation) },
            };
            entityQuery = GetEntityQuery(tempQuery);
            densityMatrix = new NativeArray<float>(Map.AllPoints, Allocator.Persistent);
            collidersDensity = new NativeArray<float>(Map.AllPoints, Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            densityMatrix.Dispose();
            collidersDensity.Dispose();
            base.OnDestroy();
        }

        [BurstCompile]
        struct ClearJob : IJobParallelFor
        {
            public NativeArray<float> array;
            public void Execute(int index)
            {
                array[index] = 0f;
            }
        }

        [BurstCompile]
        struct AddArrayJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> from;

            public NativeArray<float> to;
            public void Execute(int index)
            {
                to[index] += from[index] * 6f;
            }
        }
        private static bool hasDensityPresent = false;
        protected override void OnUpdate()
        {
            if (First)
            {
                Entities.ForEach((Entity entity, ref PathFindingData data) =>
                {
                    if (!hasDensityPresent && (data.avoidMethod == CollisionAvoidanceMethod.DensityGrid || data.avoidMethod == CollisionAvoidanceMethod.FutureAvoidance)) hasDensityPresent = true;
                });
            }
            if (!hasDensityPresent) return;

            MapChanged();

            var clearJob = new ClearJob() { array = densityMatrix };
            var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);
            var job = new SetDensityGridJob() { 
                quadrantHashMap = densityMatrix, 
                oneLayer = Map.OneLayer, 
                maxGroup = Map.MaxGroup, 
                max = Map.Values,
                WalkerHandle = GetComponentTypeHandle<Walker>(true),
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
                CollisionParametersHandle = GetComponentTypeHandle<CollisionParameters>(true),
            };
            var handle = job.Schedule(entityQuery, clearHandle);
            ForeachColliders();
            var addJob = new AddArrayJob() { from = collidersDensity, to = densityMatrix };
            var addHandle = addJob.Schedule(densityMatrix.Length, batchSize, handle);
            addHandle.Complete();
            //Debug();
        }

        private void MapChanged()
        {
            if (Map.AllPoints != densityMatrix.Length)
            {
                densityMatrix.Dispose();
                collidersDensity.Dispose();
                densityMatrix = new NativeArray<float>(Map.AllPoints, Allocator.Persistent);
                collidersDensity = new NativeArray<float>(Map.AllPoints, Allocator.Persistent);
            }
        }

        private bool First = true;
        private void ForeachColliders()
        {
            if (First)
            {
                EntityQuery entityQuery = GetEntityQuery(typeof(PhysicsCollider), typeof(LocalToWorld));
                var job = new SetDensityCollisionJob()
                {
                    densityMatrix = collidersDensity,
                    oneLayer = Map.OneLayer,
                    widthPoints = Map.WidthPoints,
                    heightPoints = Map.HeightPoints,
                    maxGroup = Map.MaxGroup,
                    max = Map.Values,
                    LocalToWorldHandle = GetComponentTypeHandle<LocalToWorld>(false),
                    PhysicsCollidertHandle = GetComponentTypeHandle<PhysicsCollider>(false)
                };
                var handle = job.Schedule(entityQuery);
                handle.Complete();
            }
            First = false;
        }

        void Debug()
        {
            int group = 1;
            for (int j = 1; j < Map.HeightPoints - 1; j++)
                for (int i = 1; i < Map.WidthPoints - 1; i++)
                {
                    float right = densityMatrix[Map.OneLayer * group + QuadrantVariables.Index(i + 1, j, Map.Values)];
                    float left = densityMatrix[Map.OneLayer * group + QuadrantVariables.Index(i - 1, j, Map.Values)];
                    float up = densityMatrix[Map.OneLayer * group + QuadrantVariables.Index(i, j + 1, Map.Values)];
                    float down = densityMatrix[Map.OneLayer * group + QuadrantVariables.Index(i, j - 1, Map.Values)];

                    var point = QuadrantVariables.ConvertToWorld(new float3(i, 0, j), Map.Values);
                    if (right < left && right < up && right < down)
                    {
                        DebugProxy.DrawLine(point, point + new float3(0.2f, 0, 0), Color.red);
                        continue;
                    }
                    if (left < up && left < down && left < right)
                    {
                        DebugProxy.DrawLine(point, point + new float3(-0.2f, 0, 0), Color.red);
                        continue;
                    }
                    if (up < down && up < right && up < left)
                    {
                        DebugProxy.DrawLine(point, point + new float3(0, 0, 0.2f), Color.red);
                        continue;
                    }
                    if (down < left && down < up && down < right)
                    {
                        DebugProxy.DrawLine(point, point + new float3(0, 0, -0.2f), Color.red);
                        continue;
                    }
                }
        }
    }
}

