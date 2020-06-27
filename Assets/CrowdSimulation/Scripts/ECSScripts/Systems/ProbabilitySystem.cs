using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
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
    class ProbabilitySystem : ComponentSystem
    {
        public static readonly int Angels = 10;
        private static int batchSize = 64;

        public static NativeArray<float> densityMatrix;

        public static NativeArray<float> collidersDensity;

        public static float3 Right => new float3((1f / Map.density), 0, 0);
        public static float3 Up => new float3(0, 0, (1f / Map.density));

        public static Entity selected = Entity.Null;

        protected override void OnCreate()
        {
            densityMatrix = new NativeArray<float>(Map.OneLayer, Allocator.Persistent);
            collidersDensity = new NativeArray<float>(Map.OneLayer, Allocator.Persistent);
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
                    if (!hasDensityPresent && data.avoidMethod == CollisionAvoidanceMethod.Probability) hasDensityPresent = true;
                });
            }
            if (!hasDensityPresent) return;

            MapChanged();

            EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker), typeof(CollisionParameters));

            var clearJob = new ClearJob() { array = densityMatrix };
            var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);

            var job = new SetProbabilityJob() { quadrantHashMap = densityMatrix, oneLayer = Map.OneLayer, max = Map.Values };

            var handle = JobForEachExtensions.Schedule(job, entityQuery, clearHandle);
            ForeachColliders();
            var addJob = new AddArrayJob() { from = collidersDensity, to = densityMatrix };
            var addHandle = addJob.Schedule(densityMatrix.Length, batchSize, handle);
            addHandle.Complete();
            Debug();
        }

        private void MapChanged()
        {
            if (Map.OneLayer != densityMatrix.Length)
            {
                densityMatrix.Dispose();
                collidersDensity.Dispose();
                densityMatrix = new NativeArray<float>(Map.OneLayer, Allocator.Persistent);
                collidersDensity = new NativeArray<float>(Map.OneLayer, Allocator.Persistent);
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
                    max = Map.Values
                };
                var handle = JobForEachExtensions.Schedule(job, entityQuery);
                handle.Complete();
            }
            First = false;
        }

        void Debug()
        {
            if (selected  != Entity.Null)
            {
                var translation = EntityManager.GetComponentData<Translation>(selected);
                var walker = EntityManager.GetComponentData<Walker>(selected);
                var collision = EntityManager.GetComponentData<CollisionParameters>(selected);

                var force = walker.direction;

                var densityB = GetDensity(collision.outerRadius, translation.Value, translation.Value + force, walker.direction);
                var point = translation.Value + force;
                DebugProxy.DrawLine(point - new float3(0, 0, 0.2f), point + new float3(0f, 0, 0.2f), new Color(0, densityB * 0.4f, 1f));
                DebugProxy.DrawLine(point - new float3(0.2f, 0, 0), point + new float3(0.2f, 0, 0), new Color(0, densityB * 0.4f, 1f));
                for (int i = 2; i < 4; i++)
                {
                    densityB *= 0.9f;
                    var multi = math.pow(0.5f, i);
                    var A = ProbabilityAvoidJob.GetDirection(force, math.PI * multi);
                    var B = ProbabilityAvoidJob.GetDirection(force, 0);
                    var C = ProbabilityAvoidJob.GetDirection(force, -math.PI * multi);

                    var densityA = GetDensity(collision.outerRadius, translation.Value, translation.Value + A, walker.direction);

                    var densityC = GetDensity(collision.outerRadius, translation.Value, translation.Value + C, walker.direction);

                    point = translation.Value + A;
                    DebugProxy.DrawLine(point - new float3(0, 0, 0.2f), point + new float3(0f, 0, 0.2f), new Color(0, densityA * 0.4f, 1f));
                    DebugProxy.DrawLine(point - new float3(0.2f, 0,0), point + new float3(0.2f, 0, 0), new Color(0, densityA * 0.4f, 1f));
                    point = translation.Value + C;
                    DebugProxy.DrawLine(point - new float3(0, 0, 0.2f), point + new float3(0f, 0, 0.2f), new Color(0, densityC * 0.4f, 1f));
                    DebugProxy.DrawLine(point - new float3(0.2f, 0, 0), point + new float3(0.2f, 0, 0), new Color(0, densityC * 0.4f, 1f));

                    if (densityA > densityC && densityB > densityC)
                    {
                        force = C * 0.8f;
                        densityB = densityC;
                    }
                    else
                    {
                        if (densityB > densityA && densityC > densityA)
                        {
                            force = A * 0.8f;
                            densityB = densityA;
                        }
                        else
                        {
                            force = B;
                        }
                    }
                }
               
            }
           
        }

        private float GetDensity(float radius, float3 position, float3 point, float3 velocity)
        {
            var index = QuadrantVariables.BilinearInterpolation(point, Map.Values);

            var density0 = densityMatrix[index.Index0] * index.percent0;
            var density1 = densityMatrix[index.Index1] * index.percent1;
            var density2 = densityMatrix[index.Index2] * index.percent2;
            var density3 = densityMatrix[index.Index3] * index.percent3;
            var ownDens = SetProbabilityJob.Value(radius, position, point, velocity);
            return (density0 + density1 + density2 + density3 - ownDens);
        }
    }
}

