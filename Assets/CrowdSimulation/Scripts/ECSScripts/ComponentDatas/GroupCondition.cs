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
        [System.NonSerialized]
        public float radius;
        public bool isSet;
    }
}
