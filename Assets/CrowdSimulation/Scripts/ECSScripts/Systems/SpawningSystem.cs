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
                Entities.ForEach((ref SpawnerParameters parameters, ref SpawnerPrefabContainer prefab, ref Translation translation, ref Rotation rotation) =>
                {
                    if (parameters.currentEntity >= parameters.maxEntity)
                    {
                        parameters.spawnTimer = parameters.spawnTime;
                        return;
                    }
                    parameters.spawnTimer -= Time.DeltaTime;
                    if (parameters.spawnTimer > 0f)
                    {
                        return;
                    }
                    parameters.spawnTimer = parameters.spawnTime;
                    parameters.currentEntity++;

                    var entity = EntityManager.Instantiate(prefab.prefab);
                    var randomSize = 1f;
                    var randomOffset = new float3(random.NextFloat(randomSize * 2) - randomSize, 0, random.NextFloat(randomSize * 2) - randomSize);
                    EntityManager.SetComponentData(entity, new Translation {
                        Value = translation.Value + parameters.offset + randomOffset
                    });
                    var fighter = new Fighter
                    {
                        groupId = parameters.groupId,
                        goalPos = translation.Value + parameters.offset,
                        state = FightState.Standing,
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
