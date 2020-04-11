using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Condition : IComponentData
    {
        public float lifeLine;
        public float hunger;
        public float thirst;
    }
}
