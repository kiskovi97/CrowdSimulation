using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct Selection : IComponentData
    {
        [System.NonSerialized]
        public bool Selected;
        [System.NonSerialized]
        public bool changed;
    }
}
