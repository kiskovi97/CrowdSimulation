using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public struct MapValues
{
    public int maxHeight;
    public int maxWidth;
    public int density;
    public int MaxGroup;
    public int heightPoints;
    public int widthPoints;
}

public class Map : MonoBehaviour
{

    public GameObject[] edibles;
    public GameObject[] obstacles;
    public float distance = 1f;
    public int ediblesNumber = 30;
    public int obstaclesNumber = 30;
    public int InputMaxHeight = 30;
    public int InputMaxWidth = 50;

    public static int MaxHeight { get => Instance != null ? Instance.InputMaxHeight : 30; }
    public static int MaxWidth { get => Instance != null ? Instance.InputMaxWidth : 50; }

    public static readonly int density = 2;
    private static readonly float outerRadius = 0.6f;
    private static readonly float innerRadius = 0.2f;

    public static int MaxGroup { get; set; } = 5;

    public static int HeightPoints { get => MaxHeight * density * 2; }
    public static int WidthPoints { get => MaxWidth * density * 2; }

    public static int OneLayer { get => HeightPoints * WidthPoints; }
    public static int AllPoints { get => OneLayer * MaxGroup; }

    private static Map Instance;

    public static MapValues Values
    {
        get => new MapValues()
        {
            maxHeight = MaxHeight,
            maxWidth = MaxWidth,
            density = density,
            MaxGroup = MaxGroup,
            heightPoints = MaxHeight * density * 2,
            widthPoints = MaxWidth * density * 2,
        };
    }

    private void Awake()
    {
        Instance = this;
    }

    List<Vector3> prevPositions;

    private void Start()
    {
        prevPositions = new List<Vector3>();

        for (int i = 0; i < obstaclesNumber; i++)
        {
            var index = (int)(Random.value * obstacles.Length);
            Vector3 pos;
            int max = 0;
            do
            {
                max++;
                pos = RandomPos();
            } while (IsNear(pos, distance * 3f) && max < 100);

            var obj = Instantiate(obstacles[index], pos, Quaternion.identity);
            prevPositions.Add(pos);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }

        for (int i = 0; i < ediblesNumber; i++)
        {
            var index = (int)(Random.value * edibles.Length);
            Vector3 pos;
            int max = 0;
            do
            {
                max++;
                pos = RandomPos();
            } while (IsNear(pos, distance) && max < 100);

            var obj = Instantiate(edibles[index], pos, Quaternion.identity);
            prevPositions.Add(pos);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }
    }

    float3 RandomPos()
    {
        var height = Random.value * MaxHeight * outerRadius * 2 - MaxHeight * outerRadius;
        var width = Random.value * MaxWidth * outerRadius * 2 - MaxWidth * outerRadius;

        var pos = new Vector3(width, 0, height);
        return pos + pos.normalized * innerRadius * MaxHeight;
    }

    bool IsNear(Vector3 pos, float dist)
    {
        foreach (var position in prevPositions)
        {
            if ((pos - position).magnitude < dist)
            {
                return true;
            }
        }
        return false;
    }
}
