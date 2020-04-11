using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    public struct Walker : IComponentData
    {
        public int broId;
        public float3 direction;
        public float maxSpeed;
    }
}
