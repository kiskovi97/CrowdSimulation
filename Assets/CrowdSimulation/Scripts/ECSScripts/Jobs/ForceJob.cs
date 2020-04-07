
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public struct ForceJob : IJobForEach<PathForce, Walker>
{
    public float deltaTime;

    public void Execute([ReadOnly] ref PathForce pathForce, ref Walker walker)
    {
        var b3 = math.isnan(pathForce.force);
        if (!b3.x && !b3.y && !b3.z)
        {
            if (math.length(pathForce.force) == 0f)
            {
                walker.direction -= walker.direction * deltaTime * 4f;
            } else
            {
                walker.direction += pathForce.force * deltaTime * 4f;
            }
        }

        var speed = math.length(walker.direction);

        if (speed > walker.maxSpeed)
        {
            walker.direction = math.normalizesafe(walker.direction) * walker.maxSpeed;
        }
    }
}
