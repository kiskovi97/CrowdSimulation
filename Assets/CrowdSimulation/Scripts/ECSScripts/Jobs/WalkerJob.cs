using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;

[BurstCompile]
public struct WalkerJob : IJobForEach<Rotation, Translation, Walker>
{
    public float deltaTime;
    public float maxWidth;
    public float maxHeight;

    public void Execute(ref Rotation rotation, ref Translation transform, [ReadOnly] ref Walker walker)
    {
        RotateForward(ref walker, ref rotation);

        Step(ref transform, walker.direction);

        EdgeReaction(ref transform);
    }

    private void RotateForward([ReadOnly] ref Walker walker, ref Rotation rotation)
    {
        var speed = math.length(walker.direction);

        if (speed > 0.1f)
        {
            var forward = new float3(0, 0, 1);
            var realForward = math.mul(rotation.Value, forward);

            var dot = (math.dot(math.normalize(realForward), math.normalize(walker.direction)) + 1f) * 0.5f; // 0-1
                    
            dot = math.min(1.0f, dot);
            dot = math.max(0.5f, dot);

            var avarage = realForward * dot + walker.direction * (1 - dot);

            rotation.Value = quaternion.LookRotationSafe(avarage, new float3(0, 1, 0));
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