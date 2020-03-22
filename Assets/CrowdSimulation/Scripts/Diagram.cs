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

    private List<Color> colors = new List<Color>();

    private List<List<Vector2>> allPoints = new List<List<Vector2>>();

    public int Register(Color color)
    {
        colors.Add(color);
        allPoints.Add(new List<Vector2>());
        return colors.Count - 1;
    }

    public void AddPoint(int id, float value)
    {
        allPoints[id].Add(new Vector2(Time.time * 1f / maxTime, value * 1f / maxValue));
    }

    // Update is called once per frame
    void Update()
    {
        DrawAxis();
        DrawPoints();
    }

    private void DrawPoints()
    {
        for (int j = 0; j < allPoints.Count; j++)
        {
            var points = allPoints[j];
            for (int i = 1; i < points.Count; i++)
            {
                var prev = new Vector3(points[i - 1].x, 0, points[i - 1].y);
                var next = new Vector3(points[i].x, 0, points[i].y);

                DrawLine(prev, next, j);
            }
        }  
    }

    void DrawAxis()
    {
        DrawAx(Vector3.left * arrowScale, Vector3.right, -1);
        DrawAx(Vector3.back * arrowScale, Vector3.forward, -1);
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
        var color = Color.white;
        if (priority < colors.Count && priority >= 0)
        {
            color = colors[priority];
        }
        var realFrom = transform.TransformPoint(from);
        var realTo = transform.TransformPoint(to);
        Debug.DrawLine(realFrom, realTo, color);
    }
}
