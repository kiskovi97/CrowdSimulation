using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static AnimatorSystem;

public struct AnimatorJob : IJobForEach<Translation, Rotation, Animator>
{
    public float deltaTime;

    [ReadOnly]
    public NativeArray<AnimationStep> steps;

    [ReadOnly]
    public NativeArray<Animation> animations;

    public void Execute(ref Translation translation, ref Rotation rotation, ref Animator animator)
    {
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
