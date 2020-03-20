
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
//World.Active.GetOrCreateManager<EntityManager>();
public class InfectionSystem : ComponentSystem
{
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
            var rendererMesh = World.DefaultGameObjectInjectionWorld.EntityManager.GetSharedComponentData<RenderMesh>(entity);
            if (c0.infectionTime > 0.2f)
            {
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                {
                    material = Materails.Instance.infected,
                    mesh = rendererMesh.mesh
                });
            } else
            {
                if (c0.reverseImmunity < 1f)
                {
                    PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                    {
                        material = Materails.Instance.immune,
                        mesh = rendererMesh.mesh
                    });
                } else
                {
                    PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                    {
                        material = Materails.Instance.notInfected,
                        mesh = rendererMesh.mesh
                    });
                }
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
