using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Burst;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct WalkerJob : IJobChunk
    {
        private static readonly float radiantperSecond = 2f;
        public float deltaTime;
        public float maxWidth;
        public float maxHeight;

        public ComponentTypeHandle<Walker> WalkerHandle;
        public ComponentTypeHandle<Rotation> RotationHandle;
        public ComponentTypeHandle<Translation> TranslationHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var walkers = chunk.GetNativeArray(WalkerHandle);
            var rotations = chunk.GetNativeArray(RotationHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var walker = walkers[i];
                var rotation = rotations[i];
                var translation = translations[i];

                Execute(ref rotation, ref translation, ref walker);

                walkers[i] = walker;
                rotations[i] = rotation;
                translations[i] = translation;
            }
        }

        public void Execute(ref Rotation rotation, ref Translation transform, ref Walker walker)
        {
            walker.direction.y = 0;

            RotateForward(walker, ref rotation);

            Step(ref transform, walker.direction);

            EdgeReaction(ref transform);
        }

        private void RotateForward(Walker walker, ref Rotation rotation)
        {
            var speed = math.length(walker.direction);

            if (speed > 0.01f)
            {
                var toward = quaternion.LookRotationSafe(walker.direction, new float3(0, 1, 0));
                rotation.Value = math.slerp(rotation.Value, toward, deltaTime * radiantperSecond);
            }
        }

        private void Step(ref Translation transform, float3 direction)
        {

            transform.Value += direction * deltaTime;
        }

        private void EdgeReaction(ref Translation transform)
        {
            if (transform.Value.x < -maxWidth)
            {
                transform.Value.x += 2 * maxWidth;
            }
            if (transform.Value.x > maxWidth)
            {
                transform.Value.x -= 2 * maxWidth;
            }
            if (transform.Value.z < -maxHeight)
            {
                transform.Value.z += 2 * maxHeight;
            }
            if (transform.Value.z > maxHeight)
            {
                transform.Value.z -= 2 * maxHeight;
            }
        }
    }
}