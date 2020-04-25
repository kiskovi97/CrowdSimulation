using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct GroupCondition : IComponentData
    {
        public float3 goalPoint;
        public float goalRadius;

        [System.NonSerialized]
        public float3 goal;
        public bool isSet;
    }
}
