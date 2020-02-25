using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PersonObject : MonoBehaviour, IConvertGameObjectToEntity
{
    public float maxSpeed = 2f;
    public float innerRadius = 1f;
    public float outerRadius = 2f;
    public float3 direction = Vector3.left;
    public float3 desire = Vector3.left;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float3 direction = transform.forward;

        dstManager.AddComponentData(entity, new Condition
        {
            hunger = 0f,
            lifeLine = 1f,
            thirst = 0f,
        });
        dstManager.AddComponentData(entity, new FoodHierarchie
        {
            hierarchieNumber = 0
        });
        dstManager.AddComponentData(entity, new GroupCondition
        {
            goalPoint = new float3(0, 0, 1)
        });
        dstManager.AddComponentData(entity, new CollisionParameters
        {
            innerRadius = innerRadius,
            outerRadius = outerRadius
        });
        dstManager.AddComponentData(entity, new Walker
        {
            direction = direction,
            maxSpeed = maxSpeed,
        });

        // Forces
        dstManager.AddComponentData(entity, new DesireForce
        {
            force = float3.zero
        });
        dstManager.AddComponentData(entity, new GroupForce
        {
            force = float3.zero
        });
        dstManager.AddComponentData(entity, new DecidedForce
        {
            force = float3.zero
        });
        dstManager.AddComponentData(entity, new PathForce
        {
            force = float3.zero
        });
        dstManager.AddComponentData(entity, new CollisionForce
        {
            force = float3.zero
        });
    }
}
