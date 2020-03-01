using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    public GameObject entityObject;

    public float3 goalPoint;

    public static int Id = 0;

    public int sizeX = 5;
    public int sizeZ = 5;

    public float distance = 1f;

    // Start is called before the first frame update
    void Start()
    {
        var cond = new GroupCondition() { goalPoint = goalPoint };
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
                }
            }
        }
    }
}
