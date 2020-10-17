using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct AnimatorJob : IJobChunk
    {
        public float deltaTime;
        public Random random;

        [ReadOnly]
        public NativeArray<AnimationStep> steps;

        [ReadOnly]
        public NativeArray<Animation> animations;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, Fighter> hashMap;

        public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<Rotation> RotationHandle;
        public ComponentTypeHandle<AnimatorData> AnimatorHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var rotations = chunk.GetNativeArray(RotationHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);
            var animators = chunk.GetNativeArray(AnimatorHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var rotation = rotations[i];
                var translation = translations[i];
                var animator = animators[i];

                Execute(ref translation, ref rotation, ref animator);

                rotations[i] = rotation;
                translations[i] = translation;
                animators[i] = animator;
            }
        }

        private void SetFromFighter(ref AnimatorData animator)
        {
            var okay = hashMap.TryGetFirstValue(animator.entityReference, out Fighter fighter, out var iterator);
            if (!okay) return;

            if (animator.animationIndex == 1 || animator.animationIndex == 3)
            {
                if (fighter.attack == AttackType.All)
                {
                    animator.animationIndex = 3;
                }
                else
                {
                    animator.animationIndex = 1;
                }
                if (fighter.state == FightState.Fight)
                {
                    if (animator.speed < 1f)
                    {
                        animator.currentTime = random.NextFloat();
                    }
                    animator.speed = 1f;
                }
                else
                {
                    animator.animationIndex = 1;
                    animator.speed = 0;
                    animator.currentTime = 0;
                }
            }

            if (animator.animationIndex == 2)
            {
                if (fighter.state == FightState.Fight && fighter.attack == AttackType.All)
                {
                    if (animator.speed < 1f)
                    {
                        animator.currentTime = random.NextFloat();
                    }
                    animator.speed = 1f;
                }
                else
                {
                    animator.speed = 0;
                    animator.currentTime = 0;
                }
            }
        }

        public void Execute(ref Translation translation, ref Rotation rotation, ref AnimatorData animator)
        {
            SetFromFighter(ref animator);

            animator.currentTime += deltaTime * animator.speed;
            if (animator.speed < 0.8f)
            {
                animator.currentTime = 0f;
            }

            var reverse = new float3(1, animator.reverseY ? -1 : 1, 1);

            for (int i = animations[animator.animationIndex].startIndex; i < animations[animator.animationIndex].endIndex; i++)
            {
                var nextTime = steps[i + 1].time;

                if (nextTime < animator.currentTime) continue;

                var prevTime = steps[i].time;
                var deltaTime = (animator.currentTime - prevTime) / (nextTime - prevTime);

                var deltaTranslation = steps[i].position * (1 - deltaTime) + steps[i + 1].position * deltaTime;

                var prev = quaternion.EulerXYZ(steps[i].rotation * reverse);
                var next = quaternion.EulerXYZ(steps[i + 1].rotation * reverse);

                var deltaRotation = math.nlerp(prev, next, deltaTime);

                translation.Value = deltaTranslation + animator.localPos;

                rotation.Value = deltaRotation;
                return;
            }
            var lastTime = steps[animations[animator.animationIndex].endIndex].time;
            if (lastTime < animator.currentTime)
            {
                animator.currentTime -= lastTime;
            }

        }
    }
}
