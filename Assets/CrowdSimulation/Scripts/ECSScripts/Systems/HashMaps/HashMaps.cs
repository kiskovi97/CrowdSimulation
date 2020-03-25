using Unity.Physics;
using Unity.Transforms;

public class InfectionHashMap : HashMapBase<Infection>
{
}

public class EdibleHashMap : HashMapBase<Edible>
{

}

public class CollidersHashMap : HashMapBase<PhysicsCollider, LocalToWorld>
{

}

public class EntitiesHashMap : HashMapBase<Rotation, Walker, CollisionParameters>
{

}
