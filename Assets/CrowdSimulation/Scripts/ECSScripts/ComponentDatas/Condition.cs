using Unity.Entities;

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
    }
}
