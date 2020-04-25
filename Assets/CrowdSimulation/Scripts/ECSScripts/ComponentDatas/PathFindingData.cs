using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct PathFindingData : IComponentData
    {
        public PathFindingMethod pathFindingMethod;
        public DecisionMethod decisionMethod;
        public float3 decidedGoal;
        public float radius;
        public float3 Force(float3 pos, float3 walkerDirection)
        {
            if (math.length(pos - decidedGoal) <= radius)
                return walkerDirection * -1;
            return math.normalizesafe(decidedGoal - pos);
        }
    }

    public enum PathFindingMethod
    {
        DensityGrid,
        Forces,
        ShortesPath,
        No
    }

    public enum DecisionMethod
    {
        Avarage,
        Max,
        Min
    }
}
