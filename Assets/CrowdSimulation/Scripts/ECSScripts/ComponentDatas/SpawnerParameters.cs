using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct SpawnerParameters : IComponentData
    {
        public Entity prefab;
        public int groupId;
        public float spawnTimer;
        public float spawnTime;
        public int maxEntity;
        public int currentEntity;
    }
}
