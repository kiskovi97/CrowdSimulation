
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct ForceJob : IJobChunk
    {
        public float deltaTime;
        public ComponentTypeHandle<Walker> WalkerHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];

                Execute(ref walker);

                walkers[i] = walker;
            }
        }

        public void Execute(ref Walker walker)
        {
            var b3 = math.isnan(walker.force);
            if (!b3.x && !b3.y && !b3.z)
            {
                walker.direction += walker.force * deltaTime * 4f;
            }

            var speed = math.length(walker.direction);

            if (speed > walker.maxSpeed)
            {
                walker.direction = math.normalizesafe(walker.direction) * walker.maxSpeed;
            }
        }
    }
}
