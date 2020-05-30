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
    }
}
