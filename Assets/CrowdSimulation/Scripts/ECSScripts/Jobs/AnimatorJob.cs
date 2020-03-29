using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using static AnimatorSystem;

public struct AnimatorJob : IJobForEach<Translation, Rotation, Animator>
{
    public float deltaTime;

    [ReadOnly]
    public NativeArray<AnimationStep> animation;

    public void Execute(ref Translation translation, ref Rotation rotation, ref Animator animator)
    {
        animator.currentTime += deltaTime * animator.speed;
        if (animator.speed < 0.3f)
        {
            animator.currentTime = 0f;
        }

        for (int i = 0; i < animation.Length - 1; i++)
        {
            var nextTime = animation[i + 1].time;

            if (nextTime < animator.currentTime) continue;

            var prevTime = animation[i].time;
            var deltaTime = (animator.currentTime - prevTime) / (nextTime - prevTime);

            var deltaTranslation = animation[i].position * (1 - deltaTime) + animation[i + 1].position * deltaTime;
            

            var deltaRotation = animation[i].rotation.value * (1 - deltaTime) + animation[i + 1].rotation.value * deltaTime;
            translation.Value = deltaTranslation + animator.localPos;

            rotation.Value.value = deltaRotation;
            return;
        }
        var lastTime = animation[animation.Length - 1].time;
        if (lastTime < animator.currentTime)
        {
            animator.currentTime -= lastTime;
        }

    }
}
