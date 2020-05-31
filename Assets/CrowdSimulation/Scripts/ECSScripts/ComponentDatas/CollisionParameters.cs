
using Unity.Entities;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct CollisionParameters : IComponentData
    {
        public float innerRadius;
        public float outerRadius;
        public int collided;
        public int near;
        public int nearOther;
    }
}
