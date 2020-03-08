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
    
    public static void DrawLine(Vector3 x, Vector3 y, Color color)
    {
        lines.Enqueue(new Line()
        {
            Color = color,
            X0 = x,
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
    }
}
