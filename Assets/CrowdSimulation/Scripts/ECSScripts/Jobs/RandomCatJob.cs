using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct RandomCatJob : IJobForEach<RandomCat, PathForce>
{
    public Random random;
    public float deltaTime;
    public void Execute([ReadOnly] ref RandomCat randomCat, ref PathForce pathForce)
    {
        if (random.NextFloat()  < deltaTime * randomCat.random)
        {
            var dir = random.NextFloat2Direction();
            pathForce.force = new float3(dir.x, 0, dir.y);
        }
    }
}
