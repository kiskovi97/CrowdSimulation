using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Fighter : IComponentData
    {
        // Variables
        public float3 restPos;
        public float restRadius;
        public AttackType attack;
        public float attackRadius;
        public int Id;
        public int groupId;
        public float attackStrength;

        //State
        public int targerGroupId;
        public int targetId;
        public float3 targetGroupPos;
        public FightState state;
    }

    public enum FightState
    {
        Rest,
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