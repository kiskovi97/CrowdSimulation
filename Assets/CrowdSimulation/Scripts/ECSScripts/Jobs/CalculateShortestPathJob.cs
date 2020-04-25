using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
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

            GetMin(ref tmp, index - 1 - values.heightPoints, math.sqrt(2));
            GetMin(ref tmp, index - 1 + values.heightPoints, math.sqrt(2));
            GetMin(ref tmp, index + 1 - values.heightPoints, math.sqrt(2));
            GetMin(ref tmp, index + 1 + values.heightPoints, math.sqrt(2));
            array[index] = tmp;
        }

        private void GetMin(ref float tmp, int index, float distance = 1)
        {
            var small = index % values.LayerSize;
            if (ShortestPathSystem.IsIn(index, values) && !collisionMatrix[small])
            {
                var next = readArray[index];
                if (!(next < 0f) && (tmp < 0f || next + 1f < tmp))
                {
                    tmp = next + distance;
                }
            }
        }
    }
}
