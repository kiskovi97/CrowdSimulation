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
        public float viewRadius;

        //State
        [System.NonSerialized]
        public int targetId;
        [System.NonSerialized]
        public FightState state;
        [System.NonSerialized]
        public float3 goalPos;
        [System.NonSerialized]
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