using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InfectionCounter : MonoBehaviour
{
    public Diagram diagram;
    // Start is called before the first frame update
    void Start()
    {
        if (diagram == null)
        {
            diagram = GetComponent<Diagram>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var quaery = em.CreateEntityQuery(typeof(Infection));

        var infections = quaery.ToComponentDataArray<Infection>(Unity.Collections.Allocator.TempJob);
        var count = 0;
        var timeLeft = 0f;
        for (int i=0; i< infections.Length; i++)
        {
            if (infections[i].infectionTime > 0f)
            {
                count++;
                timeLeft += infections[i].infectionTime;
            }
        }
        infections.Dispose();
        diagram.AddPoint(count);
        diagram.AddPoint2(timeLeft);
    }
}
