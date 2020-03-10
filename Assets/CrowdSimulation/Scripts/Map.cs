using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public static readonly int maxHeight = 20;
    public static readonly int maxWidth = 20;

    public static readonly int density = 2;

    public static readonly int MaxGroup = 5;

    public static readonly int heightPoints = maxHeight * density * 2;
    public static readonly int widthPoints = maxWidth * density * 2;
    public static readonly int OneLayer = heightPoints * widthPoints;
    public static readonly int AllPoints = OneLayer * MaxGroup;
}
