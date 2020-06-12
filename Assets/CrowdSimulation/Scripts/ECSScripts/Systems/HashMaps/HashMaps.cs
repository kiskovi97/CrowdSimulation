using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Systems
{
    public struct QuadrantData
    {
        public float3 direction;
        public float3 position;
        public int broId;
        public float radius;
    }

    public class InfectionHashMap : HashMapBase<Infection> { }

    public class EdibleHashMap : HashMapBase<Edible> { }

    public class FoodHierarchieHashMap : HashMapBase<FoodHierarchie> { }

    public class CollidersHashMap : HashMapBase<PhysicsCollider, LocalToWorld, PathCollidable> { }

    public class EntitiesHashMap : HashMapBase<CollisionParameters, Walker> { }

    public class FightersHashMap : HashMapBase<Fighter, Walker> { }
}