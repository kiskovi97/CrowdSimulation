using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Walker : IComponentData
    {
        [System.NonSerialized]
        public float3 force;

        public int broId;
        public float3 direction;
        public float maxSpeed;
    }
}
