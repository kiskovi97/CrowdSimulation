using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct FoodHierarchie : IComponentData
    {
        public int hierarchieNumber;
    }
}
