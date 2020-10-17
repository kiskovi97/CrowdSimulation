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

        public static float3 GetClosestPoint(float3 center, float3 direction, float radius, float percent, int entitiCount, float3 maxDistances)
        {
            var help = GetWidthHeight(radius, entitiCount);

            var forward = new float3(0, 0, 1);
            var right = new float3(1, 0, 0);

            var leftDownPoint = center - math.mul(quaternion.RotateY(help.insideAngle / 2f), forward) * radius;

            var worldPoint = center + direction * percent * radius;
            var localPoint = (worldPoint - leftDownPoint) / help.distance;

            var width = math.clamp(math.round(localPoint.x), 0, help.maxWidth - 1);
            var height = math.clamp(math.round(localPoint.z), 0, help.maxHeight - 1);

            return leftDownPoint + height * forward * help.distance + width * right * help.distance;
        }

        static Help GetWidthHeight(float radius, int entitiCount)
        {
            int width = (int)math.ceil(math.sqrt(entitiCount));
            int height = width;

            if (width * height < entitiCount) height++;

            float angle = math.atan((width -1) / (height - 1));
            float distance = (2 * radius * math.sin(angle)) / (height - 1);
            float insideAngle = math.PI - 2 * angle;

            return new Help() { distance = distance, insideAngle = insideAngle, maxHeight = height, maxWidth = width };
        }
    }
}
