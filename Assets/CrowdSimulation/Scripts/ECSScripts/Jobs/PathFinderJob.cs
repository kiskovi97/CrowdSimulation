using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

public struct PathFindingJob : IJobForEach<DecidedForce, CollisionParameters, Walker, Translation, PathForce>
{
    public void Execute(ref DecidedForce decidedForce, ref CollisionParameters collisionParameters, ref Walker walker, 
        ref Translation translation, ref PathForce pathForce)
    {
        pathForce.force = decidedForce.force;
    }
}
