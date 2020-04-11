using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct AnimatorData : IComponentData
    {
        public int animationIndex;
        public float speed;
        public float currentTime;
        public float3 localPos;
        public quaternion localRotation;

        public bool reverseY;
        public int entityReference;
    }
}
