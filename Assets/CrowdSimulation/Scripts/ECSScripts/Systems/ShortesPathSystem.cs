using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    class ShortesPathSystem : ComponentSystem
    {
        private static int batchSize = 64;

        public static NativeList<float> densityMatrix;
        public static NativeList<bool> collisionMatrix;
        public static NativeList<float> tempMatrix;

        public static NativeList<float3> goalPoints;

        [BurstCompile]
        struct ClearJob : IJobParallelFor
        {
            public NativeArray<float> array;
            public NativeArray<float> array2;
            public int minIndex;
            public void Execute(int index)
            {
                if (index < minIndex) return;
                array[index] = -1f;
                array2[index] = -1f;
            }
        }

        [BurstCompile]
        struct SwitchJob : IJobParallelFor
        {
            public NativeArray<float> array;
            [ReadOnly]
            public NativeArray<float> readArray;

            public void Execute(int index)
            {
                array[index] = readArray[index];
            }
        }

        [BurstCompile]
        struct CalculateJob : IJobParallelFor
        {
            public NativeArray<float> array;
            [ReadOnly]
            public NativeArray<float> readArray;
            [ReadOnly]
            public NativeArray<bool> collisionMatrix;
            public MapValues values;

            public void Execute(int index)
            {
                var tmp = readArray[index];
                GetMin(ref tmp, index - 1);
                GetMin(ref tmp, index + 1);
                GetMin(ref tmp, index - values.heightPoints);
                GetMin(ref tmp, index + values.heightPoints);
                array[index] = tmp;
            }

            private void GetMin(ref float tmp, int index)
            {
                if (IsIn(index))
                {
                    var next = readArray[index];
                    if (!(next < 0f) && (tmp < 0f || next + 1f < tmp))
                    {
                        tmp = next + 1;
                    }
                }
            }

            private bool IsIn(int index)
            {
                var small = index % values.LayerSize;
                var height = small / values.heightPoints;
                var width = small % values.heightPoints;
                return !(height < 1 || width < 1 || height > values.heightPoints - 2 || width > values.widthPoints - 2)
                    && !collisionMatrix[index];
            }
        }

        private void ForeachColliders()
        {
            EntityQuery entityQuery = GetEntityQuery(typeof(PhysicsCollider), typeof(LocalToWorld));
            var job = new SetDensityClosestPathJob()
            {
                densityMatrix = collisionMatrix,
                widthPoints = Map.WidthPoints,
                heightPoints = Map.HeightPoints,
                max = Map.Values
            };
            var handle = JobForEachExtensions.Schedule(job, entityQuery);
            handle.Complete();
        }

        public static float3 ConvertToWorld(float3 position, MapValues max)
        {
            return position * (1f / Map.density) - new float3(max.maxWidth, 0, max.maxHeight);
        }

        protected override void OnCreate()
        {
            densityMatrix = new NativeList<float>(Allocator.Persistent);
            tempMatrix = new NativeList<float>(Allocator.Persistent);
            goalPoints = new NativeList<float3>(Allocator.Persistent);
            collisionMatrix = new NativeList<bool>(Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            densityMatrix.Dispose();
            tempMatrix.Dispose();
            goalPoints.Dispose();
            collisionMatrix.Dispose();
            base.OnDestroy();
        }

        public static void AddGoalPoint(float3 goalPoint)
        {
            goalPoints.Add(goalPoint);
            var layer = new NativeArray<float>(Map.OneLayer, Allocator.TempJob);
            densityMatrix.AddRange(layer);
            var layer2 = new NativeArray<float>(Map.OneLayer, Allocator.TempJob);
            tempMatrix.AddRange(layer2);
            var collLayer = new NativeArray<bool>(Map.OneLayer, Allocator.TempJob);
            collisionMatrix.AddRange(collLayer);

            layer.Dispose();
            layer2.Dispose();
            collLayer.Dispose();
        }
        bool First = true;

        protected override void OnUpdate()
        {
            if (First)
            {
                First = false;
                var goalPoint = new float3(0, 2, 0);
                AddGoalPoint(goalPoint);

                var clearJob = new ClearJob()
                {
                    array = densityMatrix,
                    array2 = tempMatrix,
                    minIndex = densityMatrix.Length - Map.OneLayer
                };
                var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);
                clearHandle.Complete();

                var index = DensitySystem.IndexFromPosition(goalPoint, float3.zero, Map.Values);
                densityMatrix[index.key] = 0.5f;
                tempMatrix[index.key] = 0.5f;
                ForeachColliders();
            }

            var calculateJob = new CalculateJob()
            {
                array = tempMatrix,
                readArray = densityMatrix,
                collisionMatrix = collisionMatrix,
                values = Map.Values
            };
            var calculateHandle = calculateJob.Schedule(densityMatrix.Length, batchSize);
            calculateHandle.Complete();

            var switchJob = new SwitchJob()
            {
                array = densityMatrix,
                readArray = tempMatrix,
            };
            var switchHandle = switchJob.Schedule(densityMatrix.Length, batchSize);
            switchHandle.Complete();

            Debug(Map.Values);
        }

        private void Debug(MapValues values)
        {
            for (int index = 0; index < densityMatrix.Length; index += 1)
            {
                var small = index % values.LayerSize;
                var height = small / values.heightPoints;
                var width = small % values.heightPoints;
                var point = ConvertToWorld(new float3(height, 0, width), values);
                if (densityMatrix[index] > 0f)
                {
                    DebugProxy.DrawLine(point, point + new float3(0, densityMatrix[index] * 0.1f, 0), Color.black);
                }
            }
        }
    }
}
