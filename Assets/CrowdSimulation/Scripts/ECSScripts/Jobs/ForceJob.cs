
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct ForceJob : IJobForEach<Walker>
    {
        public float deltaTime;

        public void Execute(ref Walker walker)
        {
            var b3 = math.isnan(walker.force);
            if (!b3.x && !b3.y && !b3.z)
            {
                if (math.length(walker.force) == 0f)
                {
                    walker.direction -= walker.direction * deltaTime * 4f;
                }
                else
                {
                    walker.direction += walker.force * deltaTime * 4f;
                }
            }

            var speed = math.length(walker.direction);

            if (speed > walker.maxSpeed)
            {
                walker.direction = math.normalizesafe(walker.direction) * walker.maxSpeed;
            }
        }
    }
}
