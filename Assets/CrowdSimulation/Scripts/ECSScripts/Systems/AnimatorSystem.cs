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

    public struct Animation
    {
        public int startIndex;
        public int endIndex;
    }

    public static NativeArray<Animation> animations;
    public static NativeArray<AnimationStep> animationSteps;

    protected override void OnCreate()
    {
        base.OnCreate();
        animationSteps = new NativeArray<AnimationStep>(new AnimationStep[] {
            /// RABBIT STEPS
            new AnimationStep()
            {
                position = new float3(0,0,0), rotation = quaternion.EulerXYZ(- math.radians(90),0,0), time = 0f
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
            },

            /// ARM STEPS
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(0),0,0),
                time = 0f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(-90),0,0),
                time = 0.2f
            },
            new AnimationStep()
            {
                position = new float3(0f,0,0),
                rotation = quaternion.EulerXYZ(- math.radians(0),0,0),
                time = 0.4f
            }
        }, Allocator.Persistent);

        animations = new NativeArray<Animation>(new Animation[] {
            new Animation() { startIndex = 0, endIndex = 4, },
            new Animation() { startIndex = 5, endIndex = 7, },
        }, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        animationSteps.Dispose();
        animations.Dispose();
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
        var job = new AnimatorJob() { deltaTime = deltaTime, animations = animations, steps = animationSteps };
        var handle = job.Schedule(this);
        handle.Complete();

        var eqd = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Fighter) }
        };
        var query = GetEntityQuery(eqd);
        var entities = query.ToEntityArray(Allocator.TempJob);
        var fighters = query.ToComponentDataArray<Fighter>(Allocator.TempJob);

        for (int i = 0; i < entities.Length; i++)
        {
            ForeachChildren(entities[i], fighters[i].state == FightState.Fight ? 1f : 0f);
        }
        entities.Dispose();
        fighters.Dispose();
    }

    private void ForeachChildren(Entity entity, float speed)
    {
        if (!EntityManager.HasComponent<Child>(entity)) return;
        var children = EntityManager.GetBuffer<Child>(entity);
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (EntityManager.HasComponent<Animator>(child.Value))
            {
                var animator = EntityManager.GetComponentData<Animator>(child.Value);
                if (animator.speed < speed)
                    animator.currentTime = UnityEngine.Random.value;
                animator.speed = speed;
                EntityManager.SetComponentData(child.Value, animator);
            }
            ForeachChildren(child.Value, speed);
        }
    }
}
