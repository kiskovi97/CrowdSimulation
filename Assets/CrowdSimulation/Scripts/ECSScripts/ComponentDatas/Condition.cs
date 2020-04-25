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
        public bool isSet;
    }
}
