using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct PathFindingData : IComponentData
    {
        public CollisionAvoidanceMethod avoidMethod;
        public PathFindingMethod pathFindingMethod;
        public DecisionMethod decisionMethod;
        public DebugMessage lastMessage;
        public float3 decidedGoal;
        public float radius;
        public float3 decidedForce;
        public float3 Force(float3 pos, float3 walkerDirection)
        {
            if (math.length(pos - decidedGoal) <= radius)
                return walkerDirection * -1;
            return math.normalizesafe(decidedGoal - pos);
        }
    }

    public enum PathFindingMethod
    {
        No,
        AStar,
    }

    public enum CollisionAvoidanceMethod
    {
        DensityGrid,
        Forces,
        No
    }

    public enum DecisionMethod
    {
        Max,
        Min,
        ConditionOverGroup,
        GroupOverCondition,
    }

    public enum DebugMessage
    {
        NothingFound,
        OnlyGroup,
        OnlyCondition,
        All
    }
}
