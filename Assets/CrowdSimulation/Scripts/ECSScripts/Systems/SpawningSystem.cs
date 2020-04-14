using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    class SpawningSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
           
                // SPawn!
                Entities.ForEach((ref SpawnerParameters parameters, ref Translation translation, ref Rotation rotation) =>
                {
                    parameters.spawnTimer -= Time.DeltaTime;
                    if (parameters.spawnTimer > 0f)
                    {
                        return;
                    }
                    parameters.spawnTimer = parameters.spawnTime;
                    parameters.currentEntity++;
                    if (parameters.currentEntity >= parameters.maxEntity)
                    {
                        return;
                    }

                    var entity = EntityManager.Instantiate(parameters.prefab);
                    EntityManager.SetComponentData(entity, new Translation { Value = translation.Value + new float3(random.NextFloat(3f), 0, random.NextFloat(3f)) });
                    var fighter = new Fighter
                    {
                        groupId = parameters.groupId,
                        restPos = translation.Value + math.mul(rotation.Value, new float3(0,0,-4f)),
                        state = FightState.Rest,
                        attackStrength = 2f,
                        attackRadius = 2f,
                        Id = entity.Index,
                    };
                    EntityManager.AddComponentData(entity, fighter);
                    FighterEntityContainer.AddEntity(entity);
                });
            
        }

        
    }
}
