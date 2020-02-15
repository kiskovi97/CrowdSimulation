using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using ReadOnlyAttribute = System.ComponentModel.ReadOnlyAttribute;
using Unity.Burst;
using UnityEngine;

[AlwaysSynchronizeSystem]
public class FlockingSystem : JobComponentSystem
{
    public static readonly float threshold = 1f;
    public static readonly float3 center = new float3(4, 0, 4);

    [BurstCompile]
    private struct FlockingQuadrantJob : IJobForEach<Rotation, Translation, People>
    {
        public float deltaTime;

        [NativeDisableParallelForRestriction]
        [ReadOnly(true)]
        public NativeMultiHashMap<int, QudrantData> targetMap;

        public void Execute(ref Rotation rotation, ref Translation transform, ref People people)
        {
            float3 avoidanceForce = new float3();
            float3 convinientForce = new float3();
            
            int key = QuadrantSystem.GetPositionHashMapKey(transform.Value);
            if (targetMap.TryGetFirstValue(key, out QudrantData data, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var force = transform.Value - data.position;
                    var length = math.length(force);
                    if (length < threshold && length > 0)
                    {
                        if (people.debug)
                        {
                           // DebugProxy.DrawLine(transform.Value, data.position, Color.red);
                        }

                        var multiplyer = (threshold - length) / threshold;
                        avoidanceForce += math.normalize(force) * multiplyer;

                        convinientForce += data.people.direction;
                    }

                } while (targetMap.TryGetNextValue(out data, ref iterator));
            }

            var direction = people.direction;

            direction += avoidanceForce * 0.2f;// * deltaTime;
            direction += people.desire * 0.1f;// * deltaTime;
            //direction += convinientForce * deltaTime;

            if (math.length(direction) > people.maxSpeed)
            {
                direction = math.normalize(direction) * people.maxSpeed;
            } else
            {
                direction *= (1 + deltaTime);
            }

            people.direction = direction;

            rotation.Value = quaternion.LookRotation(direction, new float3(0, 1, 0));

            Step(ref transform, direction);

            EdgeReaction(ref transform);
        }

        private void Step(ref Translation transform, float3 direction)
        {
            transform.Value += direction * deltaTime;
        }

        private void EdgeReaction(ref Translation transform)
        {
            if (transform.Value.x < -10f)
            {
                transform.Value.x += 20f;
            }
            if (transform.Value.x > 10f)
            {
                transform.Value.x -= 20f;
            }
            if (transform.Value.z < -10f)
            {
                transform.Value.z += 20f;
            }
            if (transform.Value.z > 10f)
            {
                transform.Value.z -= 20f;
            }
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var deltaTime = Time.DeltaTime;

        var job = new FlockingQuadrantJob()
        {
            targetMap = QuadrantSystem.quadrantHashMap,
            deltaTime = deltaTime,
        };

        return job.Schedule(this, inputDeps);
    }
}
