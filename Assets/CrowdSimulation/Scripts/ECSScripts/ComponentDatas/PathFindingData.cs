﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct PathFindingData : IComponentData
    {
        public PathFindingMethod pathFindingMethod;
        public DecisionMethod decisionMethod;
        public float3 decidedForce;
    }

    public enum PathFindingMethod
    {
        DensityGrid,
        Forces,
        No
    }

    public enum DecisionMethod
    {
        Plus,
        Max,
        Min
    }
}
