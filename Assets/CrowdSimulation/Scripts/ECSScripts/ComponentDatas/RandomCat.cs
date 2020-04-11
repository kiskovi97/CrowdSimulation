using Unity.Entities;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [GenerateAuthoringComponent]
    public struct RandomCat : IComponentData
    {
        public float random;
    }
}