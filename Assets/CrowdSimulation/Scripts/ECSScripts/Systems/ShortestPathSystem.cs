using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
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
    class ShortestPathSystem : ComponentSystem
    {
        private static readonly int batchSize = 64;

        public static NativeList<float> densityMatrix;
        public static NativeList<bool> collisionMatrix;
        public static NativeList<float> tempMatrix;

        public static NativeList<float3> goalPoints;

        struct MinValue
        {
            public float value;
            public int index;
            public float3 offsetVector;
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

        [BurstCompile]
        struct CalculateShortestPathJob : IJobParallelFor
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
                if (tmp >= 0f) return;
                GetMin(ref tmp, index - 1);
                GetMin(ref tmp, index + 1);
                GetMin(ref tmp, index - values.heightPoints);
                GetMin(ref tmp, index + values.heightPoints);
                array[index] = tmp;
            }

            private void GetMin(ref float tmp, int index)
            {
                var small = index % values.LayerSize;
                if (IsIn(index, values) && !collisionMatrix[small])
                {
                    var next = readArray[index];
                    if (!(next < 0f) && (tmp < 0f || next + 1f < tmp))
                    {
                        tmp = next + 1;
                    }
                }
            }
        }

        private MinValue GetMinValue(int index, MapValues value)
        {
            MinValue tmp = new MinValue()
            {
                index = 0,
                value = densityMatrix[index],
                offsetVector = float3.zero,
            };
            GetMinValue(ref tmp, index - 1, value, new float3(0, 0, -1) / (float)value.density);
            GetMinValue(ref tmp, index + 1, value, new float3(0, 0, 1) / (float)value.density);
            GetMinValue(ref tmp, index - value.heightPoints, value, new float3(-1, 0, 0) / (float)value.density);
            GetMinValue(ref tmp, index + value.heightPoints, value, new float3(1, 0, 0) / (float)value.density);
            return tmp;
        }

        private void GetMinValue(ref MinValue tmp, int index, MapValues value, float3 offsetvector)
        {
            if (IsIn(index, value))
            {
                var next = densityMatrix[index];
                if (next >= 0f && next < tmp.value)
                {
                    tmp.value = next;
                    tmp.index = index;
                    tmp.offsetVector = offsetvector;
                }
            }
        }

        private static bool IsIn(int index, MapValues values)
        {
            var small = index % values.LayerSize;
            var height = small / values.heightPoints;
            var width = small % values.heightPoints;
            return !(height < 1 || width < 1 || height > values.heightPoints - 2 || width > values.widthPoints - 2);
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
            tempMatrix.AddRange(layer);
            layer.Dispose();

            DebugProxy.Log(densityMatrix.Length + " / " + densityMatrix.Capacity);
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
            if (LastGoalPointCount < goalPoints.Length)
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

            Debug(Map.Values, goalPoints.Length - 1);
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
                minIndex = densityMatrix.Length - Map.OneLayer * (goalPoints.Length - LastGoalPointCount)
            };
            var clearHandle = clearJob.Schedule(densityMatrix.Length, batchSize);
            clearHandle.Complete();

            while (LastGoalPointCount < goalPoints.Length)
            {
                var index = DensitySystem.IndexFromPosition(goalPoints[goalPoints.Length - 1], float3.zero, Map.Values);
                densityMatrix[index.key + Map.OneLayer * LastGoalPointCount] = 0.5f;
                tempMatrix[index.key + Map.OneLayer * LastGoalPointCount] = 0.5f;
                LastGoalPointCount++;
            }
        }

        private void Debug(MapValues values, int groupId)
        {
            if (goalPoints.Length == 0 || groupId < 0) return;
            for (int index = values.LayerSize * groupId; index < values.LayerSize * (groupId + 1); index += 9)
            {
                var small = index % values.LayerSize;
                var height = small / values.heightPoints;
                var width = small % values.heightPoints;
                var point = DensitySystem.ConvertToWorld(new float3(height, 0, width), values);
                if (densityMatrix[index] > 0f)
                {
                    var minValue = GetMinValue(index, Map.Values);
                    DebugProxy.DrawLine(point, point + minValue.offsetVector, Color.black);
                }
            }
        }
    }
}
