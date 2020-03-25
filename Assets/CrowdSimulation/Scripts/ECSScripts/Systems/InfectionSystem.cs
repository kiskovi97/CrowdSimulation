
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(InfectionHashMap))]
public class InfectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

        var job = new InfectionJob()
        {
            targetMap = InfectionHashMap.quadrantHashMap,
            deltaTime = Time.DeltaTime,
            random = random,
        };
        var handle = job.Schedule(this);
        handle.Complete();

        Entities.ForEach((Entity entity, ref Infection infection) =>
        {
            var rendererMesh = World.DefaultGameObjectInjectionWorld.EntityManager.GetSharedComponentData<RenderMesh>(entity);
            if (infection.infectionTime > 0.2f)
            {
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                {
                    material = Materails.Instance.infected,
                    mesh = rendererMesh.mesh
                });
            } else
            {
                if (infection.reverseImmunity < 1f)
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
}
