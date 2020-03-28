using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public struct MapValues
    {
        public int maxHeight;
        public int maxWidth;
        public int density;
        public int MaxGroup;
        public int heightPoints;
        public int widthPoints;
    }

    private static Map Instance;

    public static MapValues Values
    {
        get
        {
            var maxHeight = 30;
            var density = 2;
            var maxWidth = 50;
            if (Instance == null) return new MapValues()
            {
                maxHeight = maxHeight,
                maxWidth = maxWidth,
                density = density,
                MaxGroup = 5,
                heightPoints = maxHeight * density * 2,
                widthPoints = maxWidth * density * 2,
            };
            maxHeight = Map.maxHeight;
            density = Map.density;
            maxWidth = Map.maxWidth;
            return new MapValues()
            {
                maxHeight = maxHeight,
                maxWidth = maxWidth,
                density = density,
                MaxGroup = Map.MaxGroup,
                heightPoints = maxHeight * density * 2,
                widthPoints = maxWidth * density * 2,
            };

        }
    }

    public int InputMaxHeight = 30;
    public int InputMaxWidth = 50;

    public static int maxHeight
    {
        get
        {
            if (Instance != null)
                return Instance.InputMaxHeight;
            return 30;
        }
    }
    public static int maxWidth
    {
        get
        {
            if (Instance != null)
                return Instance.InputMaxWidth;
            return 50;
        }
    }

    public static float2 max { get => new float2(maxWidth, maxHeight); }

    public static readonly float innerRadius = 0.8f;

    public static readonly int density = 2;

    public static int MaxGroup { get; set; } = 5;

    public static int heightPoints { get => maxHeight * density * 2; }
    public static int widthPoints { get => maxWidth * density * 2; }

    public static int OneLayer { get => heightPoints * widthPoints; }
    public static int AllPoints { get => OneLayer * MaxGroup; }

    public GameObject[] edibles;
    public GameObject[] obstacles;
    public int beginingNumber = 30;
    public int obstaclesNumber = 30;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < beginingNumber; i++)
        {
            var height = Random.value * maxHeight * innerRadius * 2 - maxHeight * innerRadius;
            var width = Random.value * maxWidth * innerRadius * 2 - maxWidth * innerRadius;
            var index = (int)(Random.value * edibles.Length);
            var obj = Instantiate(edibles[index], new Vector3(width, 0, height), Quaternion.identity);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }

        for (int i = 0; i < obstaclesNumber; i++)
        {
            var height = Random.value * maxHeight * innerRadius * 2 - maxHeight * innerRadius;
            var width = Random.value * maxWidth * innerRadius * 2 - maxWidth * innerRadius;
            var index = (int)(Random.value * obstacles.Length);
            var obj = Instantiate(obstacles[index], new Vector3(width, 0, height), Quaternion.identity);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }
    }
}
