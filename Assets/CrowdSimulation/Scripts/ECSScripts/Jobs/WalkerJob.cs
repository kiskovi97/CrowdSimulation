using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;

[BurstCompile]
public struct WalkerJob : IJobForEach<Rotation, Translation, Walker>
{
    private static readonly float radiantperSecond = 2f;
    public float deltaTime;
    public float maxWidth;
    public float maxHeight;

    public void Execute(ref Rotation rotation, ref Translation transform, [ReadOnly] ref Walker walker)
    {
        walker.direction.y = 0;

        RotateForward(walker, ref rotation);

        Step(ref transform, walker.direction);

        EdgeReaction(ref transform);
    }

    private void RotateForward(Walker walker, ref Rotation rotation)
    {
        var speed = math.length(walker.direction);

        if (speed > 0.1f)
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