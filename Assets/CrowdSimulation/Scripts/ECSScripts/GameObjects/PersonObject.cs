using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PersonObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 direction;
    public float maxSpeed;
    private int broId;
    private PathFindingMethod method = PathFindingMethod.No;


    CrowdSpawner parent;
    public void ConnectParent(CrowdSpawner parent)
    {
        this.parent = parent;
    }

    private GroupCondition condition = new GroupCondition{};
    private bool conditionAdded = false;

    public void ChangeGroup(GroupCondition condition, int broId, PathFindingMethod method)
    {
        this.condition = condition;
        this.broId = broId;
        this.method = method;
        conditionAdded = true;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (conditionAdded)
        {
            dstManager.AddComponentData(entity, condition);
        }
        dstManager.AddComponentData(entity, new Walker
        {
                direction = direction,
                maxSpeed = maxSpeed,
                broId = broId,
        });
        dstManager.AddComponentData(entity, new PathFindingData
        {
            method = method,
        });

        if (parent != null)
            parent.AddEntity(entity);
    }
}
