using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.CrowdSimulation.Scripts.Utilities.CollisionCalculator;

public class CollisionDebug : MonoBehaviour
{
    public CollisionDebug[] others;
    public float radius = 1f;


    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(transform.position, transform.position+transform.forward, Color.green);
        var me = new Circle(transform.position, radius, transform.forward);
        if (others == null) return;
        foreach(var other in others)
        {
            var circle = new Circle(other.transform.position, other.radius, other.transform.forward);

            var time = CalculateCirclesCollisionTime(me, circle);
            if (time > 0)
            {
                var vector = CalculateCollisionAvoidance(me, circle, time, 10f);
                Debug.DrawLine(transform.position, transform.position + new Vector3(vector.x, 0, vector.y), Color.red);
                Debug.DrawLine(transform.position, transform.position + new Vector3(0, time * 10f, 0), Color.black);
            }
        }
    }
}
