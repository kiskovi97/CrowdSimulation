using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Condition : IComponentData
    {
        public float lifeLine;
        public float hunger;
        public float thirst;
        public float hurting;
        public float hurtingTime;
        public float maxLifeLine;
        public float healingSpeed;

        [System.NonSerialized]
        public float3 goal;
        public float3 Force(float3 pos)
        {
            return math.normalizesafe(goal - pos);
        }
    }
}
