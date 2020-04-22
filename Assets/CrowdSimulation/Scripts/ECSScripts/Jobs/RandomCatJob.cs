using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces;
using Unity.Burst;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct RandomCatJob : IJobForEach<RandomCat, Walker, Translation>
    {
        public Random random;
        public float deltaTime;
        public void Execute([ReadOnly] ref RandomCat randomCat, ref Walker walker, ref Translation translation)
        {
            if (random.NextFloat() < deltaTime * randomCat.random)
            {
                var dir = random.NextFloat2Direction();
                walker.force = new float3(dir.x, 0, dir.y);
            }
        }
    }

    [BurstCompile]
    public struct RandomCatGroupJob : IJobForEach<RandomCat, Walker, Translation, GroupCondition>
    {
        public Random random;
        public float deltaTime;
        public void Execute([ReadOnly] ref RandomCat randomCat, ref Walker walker, ref Translation translation, ref GroupCondition groupCondition)
        {
            if (random.NextFloat() < deltaTime * randomCat.random)
            {

                var pos = groupCondition.goalPoint - translation.Value;

                var distance = math.length(pos);
                walker.force += deltaTime * distance * pos * 0.03f;
            }
        }
    }
}
