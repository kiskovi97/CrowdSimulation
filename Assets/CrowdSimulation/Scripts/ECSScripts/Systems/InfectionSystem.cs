
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(InfectionHashMap))]
    public class InfectionSystem : ComponentSystem
    {
        private EntityQuery infectionEntities;
        protected override void OnCreate()
        {
            var infectionQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Infection), typeof(Translation) },
            };
            infectionEntities = GetEntityQuery(infectionQuery);

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

            var job = new InfectionJob()
            {
                targetMap = InfectionHashMap.quadrantHashMap,
                deltaTime = Time.DeltaTime,
                random = random,
                TranslationHandle = GetComponentTypeHandle<Translation>(true),
                InfectionHandle = GetComponentTypeHandle<Infection>(),
            };
            var handle = job.Schedule(infectionEntities);
            handle.Complete();

            Entities.ForEach((Entity entity, ref Infection infection) =>
            {
                var rendererMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                if (infection.infectionTime > 0.2f)
                {
                    PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                    {
                        material = Materails.Instance.infected,
                        mesh = rendererMesh.mesh
                    });
                }
                else
                {
                    if (infection.reverseImmunity < 1f)
                    {
                        PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                        {
                            material = Materails.Instance.immune,
                            mesh = rendererMesh.mesh
                        });
                    }
                    else
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
}
