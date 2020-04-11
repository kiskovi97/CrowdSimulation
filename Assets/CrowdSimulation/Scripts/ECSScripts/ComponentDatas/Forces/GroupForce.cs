using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas.Forces
{
    [GenerateAuthoringComponent]
    public struct GroupForce : IComponentData
    {
        public float3 force;
    }
}
