using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct SpawnerParameters : IComponentData
    {
        public int groupId;
        public float spawnTimer;
        public float spawnTime;
        public int maxEntity;
        public int currentEntity;
        public float3 offset;
    }
}
