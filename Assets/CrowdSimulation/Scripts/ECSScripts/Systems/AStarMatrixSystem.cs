﻿using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Jobs;
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
    [AlwaysSynchronizeSystem]
    class AStarMatrixSystem : ComponentSystem
    {
        private static readonly int batchSize = 64;

        public static NativeList<float> densityMatrix;
        public static NativeList<bool> collisionMatrix;
        public static NativeList<float> tempMatrix;

        protected override void OnCreate()
        {
            densityMatrix = new NativeList<float>(Allocator.Persistent);
            tempMatrix = new NativeList<float>(Allocator.Persistent);
            collisionMatrix = new NativeList<bool>(Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            densityMatrix.Dispose();
            tempMatrix.Dispose();
            collisionMatrix.Dispose();
            base.OnDestroy();
        }

        public struct MinValue
        {
            public float value;
            public int index;
            public float3 offsetVector;
            public float3 goalPoint;
        }

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

        private static MinValue GetMinValue(int index, MapValues value, int goalIndex, NativeList<float> matrix)
        {
            var density = matrix[index];

            MinValue tmp = new MinValue()
            {
                index = 0,
                value = density,
                offsetVector = float3.zero,
                goalPoint = ShortestPathSystem.goalPoints[goalIndex],
            };
            SetMinValue(ref tmp, index - 1, value, new float3(0, 0, -1) / (float)value.density, matrix);
            SetMinValue(ref tmp, index + 1, value, new float3(0, 0, 1) / (float)value.density, matrix);
            SetMinValue(ref tmp, index - value.heightPoints, value, new float3(-1, 0, 0) / (float)value.density, matrix);
            SetMinValue(ref tmp, index + value.heightPoints, value, new float3(1, 0, 0) / (float)value.density, matrix);

            SetMinValue(ref tmp, index - 1 + value.heightPoints, value, new float3(1, 0, -1) / (float)value.density, matrix);
            SetMinValue(ref tmp, index + 1 + value.heightPoints, value, new float3(1, 0, 1) / (float)value.density, matrix);
            SetMinValue(ref tmp, index - 1 - value.heightPoints, value, new float3(-1, 0, -1) / (float)value.density, matrix);
            SetMinValue(ref tmp, index + 1 - value.heightPoints, value, new float3(-1, 0, 1) / (float)value.density, matrix);
            return tmp;
        }

        private static void SetMinValue(ref MinValue tmp, int index, MapValues value, float3 offsetvector, NativeList<float> matrix)
        {
            if (IsIn(index, value))
            {
                var next = matrix[index];
                if (next >= 0f && next < tmp.value)
                {
                    tmp.value = next;
                    tmp.index = index;
                    tmp.offsetVector = offsetvector;
                }
            }
        }

        public static bool IsIn(int index, MapValues values)
        {
            var small = index % values.LayerSize;
            var height = small / values.heightPoints;
            var width = small % values.heightPoints;
            return !(height < 1 || width < 1 || height > values.heightPoints - 2 || width > values.widthPoints - 2);
        }

        public static void AddGoalPoint(float3 goalPoint)
        {
            var layer = new NativeArray<float>(Map.OneLayer, Allocator.TempJob);
            densityMatrix.AddRange(layer);
            tempMatrix.AddRange(layer);
            layer.Dispose();
        }

        private static int LastGoalPointCount = 0;
        private static bool ColliderUpdate = true;

        protected override void OnUpdate()
        {
            SearchForCollider();
            if (ColliderUpdate)
            {
                var collLayer = new NativeArray<bool>(Map.OneLayer, Allocator.TempJob);
                collisionMatrix.AddRange(collLayer);
                collLayer.Dispose();
                ForeachColliders();
                ColliderUpdate = false;
            }
            if (LastGoalPointCount < ShortestPathSystem.goalPoints.Length)
            {
                NewGoalPointAdded();
            }

            var calculateJob = new CalculateShortestPathJob()
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

        }

        private int colliderCount = 0;

        private void SearchForCollider()
        {
            EntityQuery entityQuery = GetEntityQuery(ComponentType.ReadOnly<PathCollidable>());
            var count = entityQuery.CalculateEntityCount();
            if (colliderCount != count)
            {
                colliderCount = count;
                ColliderUpdate = true;
                LastGoalPointCount = 0;
            }
        }

        private void ForeachColliders()
        {
            EntityQuery entityQuery = GetEntityQuery(typeof(PhysicsCollider), typeof(LocalToWorld), ComponentType.ReadOnly<PathCollidable>());
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

        private void NewGoalPointAdded()
        {
            var clearJob = new ClearJob()
            {
                array = densityMatrix,
                array2 = tempMatrix,
                minIndex = densityMatrix.Length - Map.OneLayer * (ShortestPathSystem.goalPoints.Length - LastGoalPointCount)
            };
            var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);
            clearHandle.Complete();

            while (LastGoalPointCount < ShortestPathSystem.goalPoints.Length)
            {
                var index = QuadrantVariables.IndexFromPosition(ShortestPathSystem.goalPoints[LastGoalPointCount], float3.zero, Map.Values);
                densityMatrix[index.key + Map.OneLayer * LastGoalPointCount] = 0.5f;
                tempMatrix[index.key + Map.OneLayer * LastGoalPointCount] = 0.5f;
                LastGoalPointCount++;
            }
        }

        private void Debug(MapValues values, int groupId, Color color)
        {
            DebugProxy.Log("Debug Draw Before");
            if (ShortestPathSystem.goalPoints.Length == 0 || groupId < 0) return;
            DebugProxy.Log("Debug Draw");
            for (int index = values.LayerSize * groupId; index < values.LayerSize * (groupId + 1); index += 7)
            {
                var small = index % values.LayerSize;
                var height = small / values.heightPoints;
                var width = small % values.heightPoints;
                var point = QuadrantVariables.ConvertToWorld(new float3(height, 0, width), values);
                if (densityMatrix[index] > 0f)
                {
                    var minValue = GetMinValue(index, Map.Values, groupId, densityMatrix);
                    DebugProxy.DrawLine(point, point + minValue.offsetVector, color);
                }
            }
        }
    }
}
