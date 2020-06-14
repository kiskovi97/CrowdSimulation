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
            //Debug();
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
                var pos = EntityManager.GetComponentData<Translation>(selected);
                var walker = EntityManager.GetComponentData<Walker>(selected);
                var collision = EntityManager.GetComponentData<CollisionParameters>(selected);
                
                for (int i = 0; i < ProbabilityAvoidJob.Angels; i++)
                {
                    var vector = ProbabilityAvoidJob.GetDirection(walker.direction, i * math.PI * 2f / ProbabilityAvoidJob.Angels);
                    vector *= 1.0f;

                    var dot = math.abs(i * 2 / (ProbabilityAvoidJob.Angels) - 0.5f);
                    var point = pos.Value + vector;
                    var density = GetDensity(collision.outerRadius, pos.Value, point, walker.direction) * dot;

                    if (density > 0)
                    {
                        DebugProxy.DrawLine(point - new float3(0.2f, 0, 0), point + new float3(0.2f, 0, 0), new Color(0, density, 1f));
                        DebugProxy.DrawLine(point - new float3(0, 0, 0.2f), point + new float3(0f, 0, 0.2f), new Color(0, density, 1f));
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
            return (density0 + density1 + density2 + density3 - ownDens * 10f);
        }
    }
}

