using Assets.CrowdSimulation.Scripts.ECSScripts.GameObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitiesPathQuality : MonoBehaviour
{
    public Diagram diagram;
    public List<CrowdSpawner> crowdSpawners;
    private List<int> ids;

    private void Start()
    {
        if (diagram == null)
        {
            diagram = GetComponent<Diagram>();
        }
        ids = new List<int>();
        for (int i = 0; i < crowdSpawners.Count; i++)
        {
            var crowd = crowdSpawners[i];
            var id = diagram.Register(crowd.color.color);
            ids.Add(id);
        }
    }

    private void Update()
    {
        if (diagram == null) return;

        for (int i = 0; i < crowdSpawners.Count; i++)
        {
            var crowd = crowdSpawners[i];
            if (crowd.HasEnties())
            diagram.AddPoint(ids[i], crowd.EntitiesDistanceFromGoal());
        }
    }
}
