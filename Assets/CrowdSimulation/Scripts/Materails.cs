using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Materails : MonoBehaviour
{
    public Material infected;
    public Material notInfected;
    public Material immune;

    public Material selected;
    public Material notSelected;

    public Material hurting;
    public Material healing;
    public Material rest;
    public Material toLow;

    public Material okayBuilding;
    public Material wrongBuilding;

    public static Materails Instance;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
}
