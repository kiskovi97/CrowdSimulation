using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public struct RandomCatJob : IJobForEach<RandomCat, PathForce, Translation>
{
    public Random random;
    public float deltaTime;
    public void Execute([ReadOnly] ref RandomCat randomCat, ref PathForce pathForce, ref Translation translation)
    {
        if (random.NextFloat()  < deltaTime * randomCat.random)
        {
            var dir = random.NextFloat2Direction();
            pathForce.force = new float3(dir.x, 0, dir.y);
        }
    }
}

public struct RandomCatGroupJob : IJobForEach<RandomCat, PathForce, Translation, GroupCondition>
{
    public Random random;
    public float deltaTime;
    public void Execute([ReadOnly] ref RandomCat randomCat, ref PathForce pathForce, ref Translation translation, ref GroupCondition groupCondition)
    {
        if (random.NextFloat() < deltaTime * randomCat.random)
        {

            var pos = groupCondition.goalPoint - translation.Value;

            var distance = math.length(pos);
            pathForce.force += deltaTime * distance * pos * 0.03f;
            //pathForce.force = pos;

        }
    }
}
