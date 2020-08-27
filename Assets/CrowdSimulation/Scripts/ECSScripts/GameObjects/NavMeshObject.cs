using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using Assets.CrowdSimulation.Scripts.Utilities;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class NavMeshObject : MonoBehaviour
{
    public Transform A;
    public Transform B;

    private void Start()
    {
        DijsktraSystem.AddGoalPoint(A.transform.position);
        DijsktraSystem.AddGoalPoint(B.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        var pointA = A.position;
        var pointB = B.position;
    }
}
