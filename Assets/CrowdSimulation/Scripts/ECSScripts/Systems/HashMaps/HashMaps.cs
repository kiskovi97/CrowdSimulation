using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct QuadrantData
{
    public float3 direction;
    public float3 position;
    public int broId;
    public float radius;
}

public class InfectionHashMap : HashMapBase<Infection>
{
}

public class EdibleHashMap : HashMapBase<Edible>
{

}

public class CollidersHashMap : HashMapBase<PhysicsCollider, LocalToWorld>
{

}

public class EntitiesHashMap : HashMapBase<CollisionParameters, Walker>
{

}
