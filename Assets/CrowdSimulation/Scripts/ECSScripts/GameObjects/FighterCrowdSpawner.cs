using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;
using System;
using Unity.Transforms;
using Unity.Mathematics;

public class FighterCrowdSpawner : MonoBehaviour
{
    public GameObject[] entityObjects;
    public FighterCrowdSpawner targetCrowd;
    [SerializeField]
    public PathFindingData data;
    public Transform cameraGoal;

    public static int Id = 0;
    
    [NonSerialized]
    public int myId;

    public int sizeX = 5;
    public int sizeZ = 5;

    public float distance = 1f;

    private List<Entity> entities = new List<Entity>();

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    private void Awake()
    {
        Id++;
        if (Id >= Map.MaxGroup)
        {
            Id = 1;
        }
        myId = Id;
    }

    // Start is called before the first frame update
    void Start()
    {
        var area = sizeX * sizeZ * distance * distance;
        var radius = Mathf.Sqrt(area / Mathf.PI);
        var fighterComp = new Fighter() {
            restPos = transform.position,
            restRadius = radius,
            groupId = myId,

            targerGroupId = targetCrowd.myId,
            targetId = -1,
            targetGroupPos = targetCrowd.gameObject.transform.position,
            state = FightState.Rest,
        };
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                var position = new Vector3((i - sizeX / 2) * distance, 0, (j - sizeZ / 2) * distance);
                var index = (int)( UnityEngine.Random.value * entityObjects.Length);
                var obj = Instantiate(entityObjects[index], transform);
                obj.transform.localPosition = position;
                var fighter = obj.GetComponent<FighterObject>();
                if (fighter != null)
                {
                    fighter.ChangeGroup(myId, data, fighterComp);
                    fighter.ConnectParent(this);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ChangeState(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeState(false);
        }

        SetCamera();
    }

    private void SetCamera()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        entities = entities.Where((entity) => em.Exists(entity)).ToList();
        var pos = float3.zero;
        int db = 0;
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<Translation>(entity);
            pos += data.Value;
            db++;
        }
        if (db > 0)
        {
            pos /= db;
            targetCrowd.SetTargetPosition(pos);
            if (cameraGoal != null)
            {
                cameraGoal.position = pos;
            }
        }
        else
        {
            targetCrowd.SetTargetPosition(transform.position);
        }
    }

    private void SetTargetPosition(float3 pos)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        entities = entities.Where((entity) => em.Exists(entity)).ToList();

        var area = entities.Count * distance * distance;
        var radius = Mathf.Sqrt(area / Mathf.PI);
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<Fighter>(entity);
            data.targetGroupPos = pos;
            data.restRadius = radius;
            em.SetComponentData(entity, data);
        }
    }

    private void ChangeState(bool fight)
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        entities = entities.Where((entity) => em.Exists(entity)).ToList();
        foreach (var entity in entities)
        {
            var data = em.GetComponentData<Fighter>(entity);
            data.state = fight ? FightState.GoToFight : FightState.Rest;
            em.SetComponentData(entity, data);
        }
    }

}
