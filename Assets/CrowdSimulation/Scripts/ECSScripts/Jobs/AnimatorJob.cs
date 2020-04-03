using Unity.Collections;
using Unity.Entities;
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
        

        for (int i = animations[animator.animationIndex].startIndex; i < animations[animator.animationIndex].endIndex; i++)
        {
            var nextTime = steps[i + 1].time;

            if (nextTime < animator.currentTime) continue;

            var prevTime = steps[i].time;
            var deltaTime = (animator.currentTime - prevTime) / (nextTime - prevTime);

            var deltaTranslation = steps[i].position * (1 - deltaTime) + steps[i + 1].position * deltaTime;
            

            var deltaRotation = steps[i].rotation.value * (1 - deltaTime) + steps[i + 1].rotation.value * deltaTime;
            translation.Value = deltaTranslation + animator.localPos;

            rotation.Value.value = deltaRotation;
            return;
        }
        var lastTime = steps[steps.Length - 1].time;
        if (lastTime < animator.currentTime)
        {
            animator.currentTime -= lastTime;
        }

    }
}
