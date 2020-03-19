using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using static InfectionSystem;

public struct InfectionJob : IJobForEach<Infection, Translation>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeArray<InfectionData> dataArray;

    public float deltaTime;

    public Random random;

    public void Execute(ref Infection infection, [ReadOnly] ref Translation translation)
    {
        if (infection.infectionTime > 0f)
        {
            infection.infectionTime -= deltaTime;
            if (infection.infectionTime < 0f)
            {
                infection.reverseImmunity *= 0.1f;
            }
        }
        else
        {
            for (int i = 0; i < dataArray.Length; i++)
            {
                var infectionData = dataArray[i];
                var distance = math.length(translation.Value - infectionData.translation.Value);
                if (infectionData.infection.infectionTime > 0f && distance < 1.5f)
                {
                    var value = random.NextFloat(0, 1);
                    if (value < 0.1f * deltaTime * infection.reverseImmunity)
                    {
                        infection.infectionTime = 10f;
                    }
                }
            }
        }
    }
}
