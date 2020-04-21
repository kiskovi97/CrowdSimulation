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
                    if (parameters.level > 0)
                        Spawn(ref parameters.simple, prefab.prefab_Simple, ref random, translation.Value + parameters.offset, parameters.groupId);
                    if (parameters.level > 1)
                        Spawn(ref parameters.master, prefab.prefab_Master, ref random, translation.Value + parameters.offset, parameters.groupId);
                });
            
        }

        private void Spawn(ref OneSpawnParameter parameter, Entity prefab, ref Random random, float3 position, int groupId)
        {
            if (parameter.currentEntity >= parameter.maxEntity)
            {
                parameter.spawnTimer = parameter.spawnTime;
                return;
            }
            parameter.spawnTimer -= Time.DeltaTime;
            if (parameter.spawnTimer > 0f)
            {
                return;
            }
            parameter.spawnTimer = parameter.spawnTime;
            parameter.currentEntity++;

            var entity = EntityManager.Instantiate(prefab);
            var randomSize = 1f;
            var randomOffset = new float3(random.NextFloat(randomSize * 2) - randomSize, 0, random.NextFloat(randomSize * 2) - randomSize);
            EntityManager.SetComponentData(entity, new Translation
            {
                Value = position + randomOffset
            });

            var fighter = EntityManager.GetComponentData<Fighter>(entity);
            fighter.groupId = groupId;
            fighter.goalPos = position;
            fighter.state = FightState.Standing;
            fighter.Id = entity.Index;
            EntityManager.SetComponentData(entity, fighter);

            FighterEntityContainer.AddEntity(entity);
        }

        
    }
}
