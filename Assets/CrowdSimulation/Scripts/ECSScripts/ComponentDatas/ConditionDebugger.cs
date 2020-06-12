using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct ConditionDebugger : IComponentData
    {
        public ConditionType type;
        public float sizeMultiplyer;
    }

    public enum ConditionType
    {
        Hunger,
        LifeLine
    }
}
