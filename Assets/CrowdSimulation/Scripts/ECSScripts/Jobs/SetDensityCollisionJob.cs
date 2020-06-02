using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct SetDensityCollisionJob : IJobForEach<PhysicsCollider, LocalToWorld>
    {
        private static readonly float distance = 3f;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> densityMatrix;

        public int oneLayer;
        public int widthPoints;
        public int heightPoints;
        public int maxGroup;
        public MapValues max;

        public void Execute(ref PhysicsCollider collider, ref LocalToWorld localToWorld)
        {
            var aabb = collider.Value.Value.CalculateAabb();
            for (int group = 0; group < maxGroup; group++)
                for (int j = 0; j < heightPoints - 1; j++)
                    for (int i = 0; i < widthPoints - 1; i++)
                    {
                        var point = QuadrantVariables.ConvertToWorld(new float3(i, 0, j), max);
                        var localPos = point - localToWorld.Position;
                        localPos = math.mul(math.inverse(localToWorld.Rotation), localPos);
                        if (aabb.Min.x - distance > localPos.x) continue;
                        if (aabb.Min.z - distance > localPos.z) continue;
                        if (aabb.Max.x + distance < localPos.x) continue;
                        if (aabb.Max.z + distance < localPos.z) continue;

                        if (collider.Value.Value.CalculateDistance(new PointDistanceInput()
                        {
                            Position = localPos,
                            MaxDistance = float.MaxValue,
                            Filter = CollisionFilter.Default
                        }, out DistanceHit hit))
                        {
                            if (distance - hit.Distance > 0f)
                                densityMatrix[group * oneLayer + QuadrantVariables.Index(i, j, max)] += math.max(0f, distance - hit.Distance);
                        }
                    }
        }
    }
}
