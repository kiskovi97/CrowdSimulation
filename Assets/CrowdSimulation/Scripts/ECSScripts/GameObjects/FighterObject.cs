using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class FighterObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public float maxSpeed;
    private int broId;
    private PathFindingData pathFindingData = new PathFindingData()
    {
        decisionMethod = DecisionMethod.Max,
        pathFindingMethod = PathFindingMethod.No
    };
    private Fighter fighter = new Fighter()
    {
        targetId = 0
    };

    FighterCrowdSpawner parent;
    public void ConnectParent(FighterCrowdSpawner parent)
    {
        this.parent = parent;
    }

    private bool changedGroup = false;

    public void ChangeGroup( int broId, PathFindingData pathFindingData, Fighter fighter)
    {
        this.broId = broId;
        this.pathFindingData = pathFindingData;
        this.fighter = fighter;
        changedGroup = true;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (changedGroup)
        {
            dstManager.AddComponentData(entity, fighter);
            dstManager.AddComponentData(entity, pathFindingData);
        }
        dstManager.AddComponentData(entity, new Walker
        {
            direction = float3.zero,
            maxSpeed = maxSpeed,
            broId = broId,
        });

        if (parent != null)
            parent.AddEntity(entity);
    }
}
