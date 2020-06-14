using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public class QuadrantVariables
    {
        public static readonly int quadrandCellSize = 5;
        private static readonly int quadrandMultiplyer = 100000;

        public static int GetPositionHashMapKey(float3 position)
        {
            //return 1;
            return (int)(math.floor(position.x / quadrandCellSize) + (quadrandMultiplyer * math.floor(position.z / quadrandCellSize)));
        }

        public static int GetPositionHashMapKey(float3 position, float3 distance)
        {
            //return 1;
            return GetPositionHashMapKey(position + distance * quadrandCellSize);
        }

        public struct KeyDistance
        {
            public int key;
            public float distance;
            public float3 roundedPosition;
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
            public float3 pos0;
            public float3 pos1;
            public float3 pos2;
            public float3 pos3;
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
                percent3 = (ipercent) * (jpercent),
                pos0 = ConvertToWorld(new float3(iMin, 0, jMin), max),
                pos1 = ConvertToWorld(new float3(iMax, 0, jMin), max),
                pos2 = ConvertToWorld(new float3(iMin, 0, jMax), max),
                pos3 = ConvertToWorld(new float3(iMax, 0, jMax), max),
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
                roundedPosition = ConvertToWorld(indexPosition, max),
            };
        }

        public static int Index(int i, int j, MapValues max)
        {
            return (max.heightPoints * i) + j;
        }

        private static float3 ConvertToLocal(float3 realWorldPosition, MapValues max)
        {
            return (realWorldPosition - max.offset + new float3(max.maxWidth, 0, max.maxHeight)) * Map.density;
        }

        public static float3 ConvertToWorld(float3 position, MapValues max)
        {
            return position * (1f / Map.density) - new float3(max.maxWidth, 0, max.maxHeight) + max.offset;
        }
    }
}
