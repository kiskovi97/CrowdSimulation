using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class AnimatorSystem : ComponentSystem
{
    public struct AnimationStep
    {
        public float3 position;
        public quaternion rotation;
        public float time;
    }

    public static NativeArray<AnimationStep> jumping;

    protected override void OnCreate()
    {
        base.OnCreate();
        jumping = new NativeArray<AnimationStep>(new AnimationStep[] {
            new AnimationStep()
            {
                position = new float3(0,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = quaternion.EulerXYZ(-0.4f - math.radians(90),0,0),
                time = 0.1f
            },
            new AnimationStep()
            {
                position = new float3(0,0.02f,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 0.5f
            },
            new AnimationStep()
            {
                position = new float3(0,0.002f,0),
                rotation = quaternion.EulerXYZ(0.4f - math.radians(90),0,0),
                time = 0.8f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(90),0,0),
                time = 1f
            }
        }, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        jumping.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((ref Parent parent, ref Animator animator) =>
        {
            if (EntityManager.HasComponent<Walker>(parent.Value))
            {
                var walker = EntityManager.GetComponentData<Walker>(parent.Value);
                animator.speed = math.length(walker.direction) * 2f;
            }
        });

        var deltaTime = Time.DeltaTime;
        var job = new AnimatorJob() { deltaTime = deltaTime, animation = jumping };
        var handle = job.Schedule(this);
        handle.Complete();
    }
}
