using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static readonly int maxHeight = 30;
    public static readonly int maxWidth = 50;

    public static readonly int density = 2;

    public static readonly int MaxGroup = 5;

    public static readonly int heightPoints = maxHeight * density * 2;
    public static readonly int widthPoints = maxWidth * density * 2;
    public static readonly int OneLayer = heightPoints * widthPoints;
    public static readonly int AllPoints = OneLayer * MaxGroup;

    public GameObject[] edibles;
    public GameObject[] obstacles;
    public int beginingNumber = 30;
    public int obstaclesNumber = 30;

    private void Start()
    {
        for (int i=0; i< beginingNumber; i++)
        {
            var height = Random.value * maxHeight * 2 - maxHeight;
            var width = Random.value * maxWidth * 2 - maxWidth;
            var index = (int)(Random.value * edibles.Length);
            var obj = Instantiate(edibles[index], new Vector3(width, 0, height), Quaternion.identity);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }

        for (int i = 0; i < obstaclesNumber; i++)
        {
            var height = Random.value * maxHeight * 2 - maxHeight;
            var width = Random.value * maxWidth * 2 - maxWidth;
            var index = (int)(Random.value * obstacles.Length);
            var obj = Instantiate(obstacles[index], new Vector3(width, 0, height), Quaternion.identity);
            obj.transform.Rotate(new Vector3(0, Random.value * 180, 0));
        }
    }
}
