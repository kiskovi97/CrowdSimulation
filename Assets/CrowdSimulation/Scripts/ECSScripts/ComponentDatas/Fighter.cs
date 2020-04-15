using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Fighter : IComponentData
    {
        // Variables
        public AttackType attack;
        public float attackRadius;
        public int Id;
        public int groupId;
        public float attackStrength;

        //State
        public int targetId;
        public FightState state;
        public float3 goalPos;
        public float goalRadius;
    }

    public enum FightState
    {
        Standing,
        GoToPlace,
        GoToFight,
        Fight
    }

    public enum AttackType
    {
        One,
        All,
        Mix
    }
}