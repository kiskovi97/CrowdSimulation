using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static readonly int maxHeight = 30;
    public static readonly int maxWidth = 50;

    public static readonly float innerRadius = 0.8f;

    public static readonly int density = 2;

    public static int MaxGroup { get; set; } = 5;

    public static int heightPoints { get => maxHeight* density * 2; }
    public static int widthPoints { get => maxWidth * density * 2; }

    public static int OneLayer { get => heightPoints * widthPoints; }
    public static int AllPoints { get => OneLayer * MaxGroup; }

    public GameObject[] edibles;
    public GameObject[] obstacles;
    public int beginingNumber = 30;
    public int obstaclesNumber = 30;

    private void Start()
    {
        for (int i=0; i< beginingNumber; i++)
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
