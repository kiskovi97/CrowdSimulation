using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Materails : MonoBehaviour
{
    public Material infected;
    public Material notInfected;
    public Material immune;
    public static Materails Instance;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
}
