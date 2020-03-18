
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
//World.Active.GetOrCreateManager<EntityManager>();
public class InfectionSystem : ComponentSystem
{
    public struct InfectionJob : IJobForEach<Infection, Translation>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeArray<InfectionData> dataArray;

        public float deltaTime;

        public Random random;

        public void Execute(ref Infection infection, ref Translation translation)
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

    public struct InfectionData
    {
        [ReadOnly]
        public Translation translation;
        [ReadOnly]
        public Infection infection;
    }


    [NativeDisableParallelForRestriction]
    private NativeArray<InfectionData> dataArray;

    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(Infection), typeof(Translation));
        var count = entityQuery.CalculateEntityCount();
        if (count != dataArray.Length)
        {
            dataArray.Dispose();
            dataArray = new NativeArray<InfectionData>(count, Allocator.Persistent);
        }

        var infections = entityQuery.ToComponentDataArray<Infection>(Allocator.TempJob);
        var translations = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        for (int i = 0; i < count; i++)
        {
            dataArray[i] = new InfectionData()
            {
                translation = translations[i],
                infection = infections[i],
            };
        }
        infections.Dispose();
        translations.Dispose();

        var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

        var job = new InfectionJob()
        {
            dataArray = dataArray,
            deltaTime = Time.DeltaTime,
            random = random,
        };
        var handle = job.Schedule(this);
        handle.Complete();

        Entities.ForEach((Entity entity, ref Infection c0) =>
        {
            if (c0.infectionTime > 0.2f)
            {
                var rendererMesh = World.DefaultGameObjectInjectionWorld.EntityManager.GetSharedComponentData<RenderMesh>(entity);
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                {
                    material = Materails.Instance.infected,
                    mesh = rendererMesh.mesh
                });
            } else
            {
                var rendererMesh = World.DefaultGameObjectInjectionWorld.EntityManager.GetSharedComponentData<RenderMesh>(entity);
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                {
                    material = Materails.Instance.notInfected,
                    mesh = rendererMesh.mesh
                });
            }
        });

    }

    protected override void OnCreate()
    {
        dataArray = new NativeArray<InfectionData>(0, Allocator.Persistent);
        base.OnCreate();
    }


    protected override void OnDestroy()
    {
        dataArray.Dispose();
        base.OnDestroy();
    }
}
