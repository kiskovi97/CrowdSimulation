using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public struct WalkerJob : IJobForEach<Rotation, Translation, Walker>
{
    public float deltaTime;

    public void Execute(ref Rotation rotation, ref Translation transform, ref Walker walker)
    {
        var speed = math.length(walker.direction);
        if (speed > 0.1f)
        {
            rotation.Value = quaternion.LookRotationSafe(walker.direction, new float3(0, 1, 0));
        }
        if (speed > walker.maxSpeed)
        {
            walker.direction = math.normalize(walker.direction) * walker.maxSpeed;
        }

        Step(ref transform, walker.direction);

        EdgeReaction(ref transform);
    }


    private void Step(ref Translation transform, float3 direction)
    {
        
        transform.Value += direction * deltaTime;
    }

    private void EdgeReaction(ref Translation transform)
    {
        if (transform.Value.x < -Map.max)
        {
            transform.Value.x += 2 * Map.max;
        }
        if (transform.Value.x > Map.max)
        {
            transform.Value.x -= 2 * Map.max;
        }
        if (transform.Value.z < -Map.max)
        {
            transform.Value.z += 2 * Map.max;
        }
        if (transform.Value.z > Map.max)
        {
            transform.Value.z -= 2 * Map.max;
        }
    }
}