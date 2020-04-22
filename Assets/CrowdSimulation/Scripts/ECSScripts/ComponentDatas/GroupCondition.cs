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

        public float3 Force(float3 pos)
        {
            return math.normalizesafe(goal - pos);
        }
    }
}
