using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class QuadrantVariables
{
    private static readonly int quadrandCellSize = 5;
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
