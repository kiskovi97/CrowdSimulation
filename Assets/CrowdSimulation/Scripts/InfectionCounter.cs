using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InfectionCounter : MonoBehaviour
{
    public Diagram diagram;

    public Color infectedColor;
    public Color immuneColor;

    int infectedId;
    int immuneId;
    // Start is called before the first frame update
    void Start()
    {
        if (diagram == null)
        {
            diagram = GetComponent<Diagram>();
        }

        infectedId = diagram.Register(infectedColor);
        immuneId = diagram.Register(immuneColor);
    }

    // Update is called once per frame
    void Update()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var quaery = em.CreateEntityQuery(typeof(Infection));

        var infections = quaery.ToComponentDataArray<Infection>(Unity.Collections.Allocator.TempJob);
        var count = 0;
        var immunes = 0;
        for (int i=0; i< infections.Length; i++)
        {
            if (infections[i].infectionTime > 0f)
            {
                count++;
            }
            if (infections[i].reverseImmunity < 1f)
            {
                immunes ++;
            }
        }
        infections.Dispose();
        diagram.AddPoint(infectedId, count);
        diagram.AddPoint(immuneId, immunes);
    }
}
