using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks.ForationHelpers
{
    static class SquereFormationHelper
    {
        public struct Help
        {
            public float distance;
            public float insideAngle;
            public int maxWidth;
            public int maxHeight;
        }

        public static float3 GetGoalPosition(float3 center, float radius, float3 direction, GroupSystem.DistanceData data)
        {
            var normalizedX = direction.x / (data.absAvarageDistanceXZ.x * 2);
            var normalizedY = direction.z / (data.absAvarageDistanceXZ.y * 2);

            var entitiyWidth = math.sqrt(data.groupSize);
            normalizedX = Round(normalizedX, entitiyWidth, false);
            normalizedY = Round(normalizedY, entitiyWidth, false);

            var width = radius;

            return center + new float3(normalizedX, 0, normalizedY) * width;
        }

        private static float Round(float normalizedX, float entitiyWidth, bool even)
        {
            var help = entitiyWidth / 2f;
            if (math.abs(normalizedX) > (1f - 1f / help))
                normalizedX = normalizedX > 0f ? 1f : -1f;
            else
            {
                normalizedX = normalizedX / 2f + 0.5f;
                normalizedX = math.round((entitiyWidth-1) * normalizedX) / (entitiyWidth-1);
                normalizedX = normalizedX * 2f - 1f;
            }
            return normalizedX;
        }
    }
}
