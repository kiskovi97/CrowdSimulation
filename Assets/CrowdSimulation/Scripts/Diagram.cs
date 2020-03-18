using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diagram : MonoBehaviour
{
    public float arrowScale = 0.2f;
    public float maxValue = 20f;
    public float maxValue2 = 40f;
    public float maxTime = 20f;

    private List<Vector2> points;
    private List<Vector2> points2;

    public void AddPoint(float value)
    {
        points.Add(new Vector2(Time.time * 1f / maxTime, value * 1f / maxValue));
    }
    
    public void AddPoint2(float value)
    {
        points2.Add(new Vector2(Time.time * 1f / maxTime, value * 1f / maxValue2));
    }

    // Start is called before the first frame update
    void Start()
    {
        points = new List<Vector2>
        {
            new Vector2(0,0),
        };

        points2 = new List<Vector2>
        {
            new Vector2(0,0),
        };
    }

    // Update is called once per frame
    void Update()
    {
        DrawAxis();
        DrawPoints2();
        DrawPoints();
    }

    private void DrawPoints()
    {
        for (int i=1; i<points.Count; i++)
        {
            var prev = new Vector3(points[i - 1].x, 0, points[i - 1].y);
            var next = new Vector3(points[i].x, 0, points[i].y);

            DrawLine(prev, next, 1);
        }
    }

    private void DrawPoints2()
    {
        for (int i = 1; i < points2.Count; i++)
        {
            var prev = new Vector3(points2[i - 1].x, 0, points2[i - 1].y);
            var next = new Vector3(points2[i].x, 0, points2[i].y);

            DrawLine(prev, next, 2);
        }
    }

    void DrawAxis()
    {
        DrawAx(Vector3.left * arrowScale, Vector3.right, 0);
        DrawAx(Vector3.back * arrowScale, Vector3.forward, 0);
    }

    void DrawAx(Vector3 from, Vector3 to, int prio)
    {
        var direction = from - to;
        var realDirection = from - to;
        var tmp = direction.x;
        direction.x = direction.z;
        direction.z = tmp * -1;
        direction.Normalize();
        realDirection.Normalize();

        direction *= arrowScale;
        realDirection *= arrowScale;

        DrawLine(from, to, prio);
        DrawLine(to + realDirection + direction, to, prio);
        DrawLine(to + realDirection - direction, to, prio);
    }

    void DrawLine(Vector3 from, Vector3 to, int priority)
    {
        var color = Color.black;
        switch(priority)
        {
            case 0: color = Color.black; break;
            case 1: color = Color.red; break;
            case 2: color = Color.blue; break;
        }
        Debug.DrawLine(transform.TransformPoint(from),transform.TransformPoint(to), color);
    }
}
