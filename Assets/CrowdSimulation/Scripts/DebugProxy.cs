using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct Line
{
    public float3 X0;
    public float3 X1;
    public Color Color;
}

public class DebugProxy : MonoBehaviour
{
    private readonly static Queue<Line> lines = new Queue<Line>();
    private readonly static Queue<string> cLines = new Queue<string>();

    public static void Log(string line)
    {
        cLines.Enqueue(line);
    }

    public static void DrawLine(Vector3 x, Vector3 y, Color color)
    {
        var left = (x - y);
        var tmp = left.x;
        left.x = left.z;
        left.z = -tmp;

        lines.Enqueue(new Line()
        {
            Color = color,
            X0 = x,
            X1 = y,
        });

        lines.Enqueue(new Line()
        {
            Color = color,
            X0 = (x + y) * 0.5f + left * 0.3f,
            X1 = y,
        });

        lines.Enqueue(new Line()
        {
            Color = color,
            X0 = (x + y) * 0.5f - left * 0.3f,
            X1 = y,
        });
    }

    // Update is called once per frame
    void Update()
    {
        while (lines.Count > 0)
        {
            var line = lines.Dequeue();
            Debug.DrawLine(line.X0, line.X1, line.Color, Time.deltaTime, true);
        }
        while (cLines.Count > 0)
        {
            Debug.Log(cLines.Dequeue());
        }
    }
}
