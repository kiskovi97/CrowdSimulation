using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    struct SetDensityClosestPathJob : IJobForEach<PhysicsCollider, LocalToWorld>
    {
        private static readonly float distance = 1f;
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> densityMatrix;

        public int widthPoints;
        public int heightPoints;
        public MapValues max;

        public void Execute(ref PhysicsCollider collider, ref LocalToWorld localToWorld)
        {
            var aabb = collider.Value.Value.CalculateAabb();
            for (int j = 0; j < heightPoints - 1; j++)
                for (int i = 0; i < widthPoints - 1; i++)
                {
                    var point = DensitySystem.ConvertToWorld(new float3(i, 0, j), max);
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
                            densityMatrix[DensitySystem.Index(i, j, max)] |= 0f < distance - hit.Distance;
                    }
                }
        }
    }
}
