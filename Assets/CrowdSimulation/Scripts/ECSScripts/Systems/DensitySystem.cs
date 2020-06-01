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
    class DensitySystem : ComponentSystem
    {
        private static int batchSize = 64;

        public static NativeArray<float> densityMatrix;

        public static NativeArray<float> collidersDensity;

        public static float3 Right => new float3((1f / Map.density), 0, 0);
        public static float3 Up => new float3(0, 0, (1f / Map.density));

        protected override void OnCreate()
        {
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

        private static float3 ConvertToLocal(float3 realWorldPosition, MapValues max)
        {
            return (realWorldPosition - max.offset + new float3(max.maxWidth, 0, max.maxHeight)) * Map.density;
        }

        public static float3 ConvertToWorld(float3 position, MapValues max)
        {
            return position * (1f / Map.density) - new float3(max.maxWidth, 0, max.maxHeight) + max.offset;
        }

        public struct KeyDistance
        {
            public int key;
            public float distance;
        }

        public struct BilinearData
        {
            public int Index0;
            public int Index1;
            public int Index2;
            public int Index3;
            public float percent0;
            public float percent1;
            public float percent2;
            public float percent3;
        }

        public static BilinearData BilinearInterpolation(float3 position, MapValues max)
        {
            var indexPosition = ConvertToLocal(position, max);
            var iMin = math.clamp((int)math.floor(indexPosition.x), 0, max.widthPoints - 1);
            var iMax = math.clamp((int)math.ceil(indexPosition.x), 0, max.widthPoints - 1);
            var jMin = math.clamp((int)math.floor(indexPosition.z), 0, max.heightPoints - 1);
            var jMax = math.clamp((int)math.ceil(indexPosition.z), 0, max.heightPoints - 1);
            var ipercent = indexPosition.x - iMin;
            var jpercent = indexPosition.z - jMin;

            return new BilinearData()
            {
                Index0 = Index(iMin, jMin, max),
                Index1 = Index(iMax, jMin, max),
                Index2 = Index(iMin, jMax, max),
                Index3 = Index(iMax, jMax, max),
                percent0 = (1f - ipercent) * (1f - jpercent),
                percent1 = (ipercent) * (1f - jpercent),
                percent2 = (1f - ipercent) * (jpercent),
                percent3 = (ipercent) * (jpercent)
            };

        }

        public static KeyDistance IndexFromPosition(float3 realWorldPosition, float3 prev, MapValues max)
        {
            var indexPosition = ConvertToLocal(realWorldPosition, max);
            var i = math.clamp((int)math.round(indexPosition.x), 0, max.widthPoints - 1);
            var j = math.clamp((int)math.round(indexPosition.z), 0, max.heightPoints - 1);
            return new KeyDistance()
            {
                key = Index(i, j, max),
                distance = math.length(ConvertToLocal(prev, max) - math.round(indexPosition)),
            };
        }

        public static int Index(int i, int j, MapValues max)
        {
            return (max.heightPoints * i) + j;
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
                    if (!hasDensityPresent && data.avoidMethod == CollisionAvoidanceMethod.DensityGrid) hasDensityPresent = true;
                });
            }
            if (!hasDensityPresent) return;

            MapChanged();

            EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(Walker), typeof(CollisionParameters));

            var clearJob = new ClearJob() { array = densityMatrix };
            var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);
            var job = new SetDensityGridJob() { quadrantHashMap = densityMatrix, oneLayer = Map.OneLayer, maxGroup = Map.MaxGroup, max = Map.Values };
            var handle = JobForEachExtensions.Schedule(job, entityQuery, clearHandle);
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
                    max = Map.Values
                };
                var handle = JobForEachExtensions.Schedule(job, entityQuery);
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
                    float right = densityMatrix[Map.OneLayer * group + Index(i + 1, j, Map.Values)];
                    float left = densityMatrix[Map.OneLayer * group + Index(i - 1, j, Map.Values)];
                    float up = densityMatrix[Map.OneLayer * group + Index(i, j + 1, Map.Values)];
                    float down = densityMatrix[Map.OneLayer * group + Index(i, j - 1, Map.Values)];

                    var point = ConvertToWorld(new float3(i, 0, j), Map.Values);
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

