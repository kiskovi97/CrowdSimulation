using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    public GameObject entityObject;

    public float3 goalPoint;
    public float goalRadius;

    public static int Id = 0;

    public int sizeX = 5;
    public int sizeZ = 5;

    public float distance = 1f;

    private List<Entity> entities = new List<Entity>();
    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    // Start is called before the first frame update
    void Start()
    {
        var cond = new GroupCondition() { goalPoint = goalPoint, goalRadius = goalRadius };
        Id++;
        for (int i = 0; i<sizeX; i++)
        {
            for (int j= 0; j<sizeZ; j++)
            {
                var position = new Vector3((i - sizeX/2) * distance, 0, (j - sizeZ / 2) * distance);
                var obj = Instantiate(entityObject, transform);
                obj.transform.localPosition = position;
                var people = obj.GetComponent<PeopleAuth>();
                if (people != null)
                {
                    people.crowdId = Id;
                }
                var person = obj.GetComponent<PersonObject>();
                if (person != null)
                {
                    person.ChangeGroup(cond);
                    person.broId = Id;
                    person.ConnectParent(this);
                }
            }
        }
    }

    private void Update()
    {
        Debug.Log(entities.Count);

        if (Input.GetKeyDown(KeyCode.A))
        {
            NewGoal();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            NewGoal2();
        }
    }

    private void NewGoal()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<GroupCondition>(entity);
            data.goalPoint = new float3(0, 0, 0);
            data.goalRadius = 10f;
            em.SetComponentData(entity, data);
        }
    }

    private void NewGoal2()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<GroupCondition>(entity);
            data.goalPoint *= -1;
            em.SetComponentData(entity, data);
        }
    }
}
