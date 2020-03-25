using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public struct InfectionJob : IJobForEach<Infection, Translation>
{
    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public NativeMultiHashMap<int, InfectionHashMap.MyData> targetMap;

    public float deltaTime;
    public Random random;

    public void Execute(ref Infection infection, [ReadOnly] ref Translation translation)
    {
        if (infection.infectionTime > 0f)
        {
            infection.infectionTime -= deltaTime;
            if (infection.infectionTime < 0f)
            {
                infection.reverseImmunity *= Infection.immunityMultiplyer;
            }
        }
        else
        {
            ForeachAround(ref infection, ref translation);
        }
    }

    private void ForeachAround(ref Infection infection, ref Translation translation)
    {
        var position = translation.Value;
        var key = QuadrantVariables.GetPositionHashMapKey(position);
        Foreach(key, ref infection, ref translation);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
        Foreach(key, ref infection, ref translation);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
        Foreach(key, ref infection, ref translation);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
        Foreach(key, ref infection, ref translation);
        key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
        Foreach(key, ref infection, ref translation);
    }

    private void Foreach(int key, ref Infection infection, ref Translation translation)
    {
        if (targetMap.TryGetFirstValue(key, out InfectionHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
        {
            do
            {
                InForeach(ref infection, ref translation, other);

            } while (targetMap.TryGetNextValue(out other, ref iterator));
        }
    }

    private void InForeach(ref Infection infection, ref Translation translation, InfectionHashMap.MyData other)
    {
        var infectionData = other;
        var distance = math.length(translation.Value - infectionData.position);
        if (infectionData.data.infectionTime > 0f && distance < Infection.infectionDistance)
        {
            var value = random.NextFloat(0, 1);
            if (value < Infection.infectionChance * deltaTime * infection.reverseImmunity)
            {
                infection.infectionTime = Infection.illTime;
            }
        }
    }
}
