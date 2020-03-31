using Unity.Entities;
using UnityEngine;

public class InfectionCounter : MonoBehaviour
{
    public Diagram diagram;

    public CrowdSpawner[] crowds;

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

        if (crowds.Length > 0)
        {
            var mInfected = 0;
            var mImmune = 0;
            foreach(var crowd in crowds)
            {
                var (infected, immune) = crowd.GetEntities(em);
                mInfected += infected;
                mImmune += immune;
            }

            diagram.AddPoint(infectedId, mInfected);
            diagram.AddPoint(immuneId, mImmune);
            return;
        }
    }
}
