using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using System.Linq;

public class FighterCrowdSpawner : MonoBehaviour
{
    public GameObject entityObject;
    public FighterCrowdSpawner targetCrowd;
    [SerializeField]
    public PathFindingData data;
    public Material color;

    public static int Id = 0;
    
    [System.NonSerialized]
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
                var obj = Instantiate(entityObject, transform);
                obj.transform.localPosition = position;
                var renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null && color != null)
                {
                    renderer.material = color;
                }
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
            NewGoal(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGoal(false);
        }
    }

    private void NewGoal(bool fight)
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
