using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct SpawnerParameters : IComponentData
    {
        public int groupId;
        public int level;
        public float3 offset;
        public OneSpawnParameter simple;
        public OneSpawnParameter master;
    }

    public struct OneSpawnParameter
    {
        public float spawnTime;
        public float spawnTimer;
        public int maxEntity;
        public int currentEntity;
    }
}
