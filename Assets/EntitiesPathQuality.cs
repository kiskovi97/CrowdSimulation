using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitiesPathQuality : MonoBehaviour
{
    public Diagram diagram;
    public CrowdSpawner crowd1;
    public CrowdSpawner crowd2;
    public CrowdSpawner crowd3;
    public CrowdSpawner crowd4;

    private void Start()
    {
        if (diagram == null)
        {
            diagram = GetComponent<Diagram>();
        }
    }

    private void Update()
    {
        if (diagram == null) return;
        if (crowd1 != null)
            diagram.AddPoint(crowd1.EntitiesDistanceFromGoal());
        if (crowd2 != null)
            diagram.AddPoint2(crowd2.EntitiesDistanceFromGoal());
        if (crowd3 != null)
            diagram.AddPoint3(crowd3.EntitiesDistanceFromGoal());
        if (crowd4 != null)
            diagram.AddPoint4(crowd4.EntitiesDistanceFromGoal());
    }
}
