using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using TMPro;

public class CrowdSpawner : MonoBehaviour
{
    public GameObject entityObject;
    public Transform goalPoint;
    public PathFindingMethod method;
    public TextMeshProUGUI info;

    public float goalRadius;
    
    public static int Id = 0;

    public int sizeX = 5;
    public int sizeZ = 5;

    public float distance = 1f;

    private List<Entity> entities = new List<Entity>();

    public float EntitiesDistanceFromGoal()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var avrgDistance = 0f;
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<Translation>(entity);
            var pos = data.Value;
            var distance = (new Vector3(pos.x, pos.y, pos.z) - goalPoint.position).magnitude;
            avrgDistance += math.max(0f,(distance - goalRadius) / (float)entities.Count);
        }
        return avrgDistance;
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    // Start is called before the first frame update
    void Start()
    {
        var cond = new GroupCondition() { goalPoint = goalPoint.position, goalRadius = goalRadius };
        Id++;
        if (Id >= Map.MaxGroup)
        {
            Id = 1;
        }
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
                    person.ChangeGroup(cond, Id, method);
                    person.ConnectParent(this);
                }
            }
        }
    }

    private void Update()
    {
        if (info != null)
        {
            info.text = "PathFinding: " + method.ToString();
        }

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
