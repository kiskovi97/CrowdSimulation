using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using ReadOnlyAttribute = System.ComponentModel.ReadOnlyAttribute;
using Unity.Burst;

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

            ForeachAround(new QudrantData() { position = transform.Value, people = people, }, ref avoidanceForce, ref convinientForce);

            var direction = people.direction;

            direction += avoidanceForce * 20f * deltaTime;// * deltaTime;

            if (people.desire.w == 0)
            {
                direction += people.desire.xyz * deltaTime * 2f;// * deltaTime;
            } else
            {
                var desire = (people.desire.xyz - transform.Value);
                if (math.length(desire) > 1f)
                {
                    desire = math.normalize(desire);
                }
                direction += desire * deltaTime * 2f;// * deltaTime;
            }

            if (math.length(direction) > people.maxSpeed)
            {
                direction = math.normalize(direction) * people.maxSpeed;
            } else
            {
                direction *= (1 + deltaTime);
            }

            people.direction = direction;

            if (math.length(direction) > 0.5f)
            {
                rotation.Value = quaternion.LookRotationSafe(direction, new float3(0, 1, 0));
            }

            Step(ref transform, direction);

            EdgeReaction(ref transform);
        }

        private void ForeachAround(QudrantData me, ref float3 avoidanceForce, ref float3 convinientForce)
        {
            var position = me.position;
            var key = QuadrantSystem.GetPositionHashMapKey(position);
            Foreach(key, me, ref avoidanceForce, ref convinientForce);
            key = QuadrantSystem.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce);
            key = QuadrantSystem.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref avoidanceForce, ref convinientForce);
            key = QuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce);
            key = QuadrantSystem.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref avoidanceForce, ref convinientForce);
        }


        private void Foreach(int key, QudrantData me, ref float3 avoidanceForce, ref float3 convinientForce)
        {
            if (targetMap.TryGetFirstValue(key, out QudrantData data, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var force = me.position - data.position;
                    var length = math.length(force);
                    var thresholdMultiplyer = data.people.crowdId == me.people.crowdId ? 1f : 1.6f;
                    if (length < threshold * thresholdMultiplyer && length > 0)
                    {
                        var distanceNormalized = (threshold * thresholdMultiplyer - length) / (threshold);

                        var frontMultiplyer = (math.dot(math.normalize(-force), math.normalize(me.people.direction)) + 1f) * 0.5f;

                        var forceMultiplyer = math.length(data.people.direction) + 0.7f;

                        var multiplyer = distanceNormalized * frontMultiplyer * forceMultiplyer;

                        var multiplyerSin = math.sin(multiplyer * math.PI / 2f);

                        avoidanceForce += math.normalize(force) * multiplyerSin;

                        convinientForce += data.people.direction;
                    }
                } while (targetMap.TryGetNextValue(out data, ref iterator));
            }
        }

        private void Step(ref Translation transform, float3 direction)
        {
            transform.Value += direction * deltaTime;
        }

        private void EdgeReaction(ref Translation transform)
        {
            if (transform.Value.x < -Map.max)
            {
                transform.Value.x += 2 * Map.max;
            }
            if (transform.Value.x > Map.max)
            {
                transform.Value.x -= 2 * Map.max;
            }
            if (transform.Value.z < -Map.max)
            {
                transform.Value.z += 2 * Map.max;
            }
            if (transform.Value.z > Map.max)
            {
                transform.Value.z -= 2 * Map.max;
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
