﻿using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Physics;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct CollisionResolveJob : IJobChunk
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, EntitiesHashMap.MyData> targetMap;

        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, CollidersHashMap.MyData> colliders;

        public float deltaTime;

        public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<Walker> WalkerHandle;
        public ComponentTypeHandle<CollisionParameters> CollisionParametersHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var collisions = chunk.GetNativeArray(CollisionParametersHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);
            var walkers = chunk.GetNativeArray(WalkerHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var collision = collisions[i];
                var translation = translations[i];
                var walker = walkers[i];

                Execute(ref translation, ref walker, ref collision);

                collisions[i] = collision;
                translations[i] = translation;
                walkers[i] = walker;
            }
        }

        public void Execute(ref Translation translation, ref Walker walker, ref CollisionParameters collision)
        {
            float3 correction = float3.zero;
            collision.near = 0;
            collision.nearOther = 0;
            collision.collided = 0;
            ForeachAround(new EntitiesHashMap.MyData() { position = translation.Value, data = collision, data2 = walker },
                ref correction, ref collision);

            ForeachAround2(collision, translation, ref correction);

            if (correction.x == float.NaN || correction.y == float.NaN || correction.y == float.NaN)
            {
                return;
            }

            correction.y = 0;

            translation.Value += correction * 0.2f;

            if (math.length(correction) > 0.1f)
            {
                walker.direction += correction * deltaTime;
            }
        }

        private void ColliderResolve(CollidersHashMap.MyData collider, CollisionParameters collision, Translation translation, ref float3 correction)
        {
            if (collider.data.Value.Value.Filter.CollidesWith == 4)
            {
                return;
            }
            var localPos = translation.Value - collider.data2.Position;
            localPos = math.mul(math.inverse(collider.data2.Rotation), localPos);

            var aab = collider.data.Value.Value.CalculateAabb();
            var distance = math.length(aab.Max - aab.Min);

            if (math.length(localPos) < distance)
            {
                if (collider.data.Value.Value.CalculateDistance(new PointDistanceInput()
                {
                    Position = localPos,
                    MaxDistance = float.MaxValue,
                    Filter = CollisionFilter.Default
                }, out DistanceHit hit))
                {
                    if (hit.Distance < collision.innerRadius * 2)
                    {
                        var normal = math.mul(collider.data2.Rotation, hit.SurfaceNormal);
                        normal.y = 0;
                        correction += math.normalizesafe(normal) * (collision.innerRadius * 2 - hit.Distance + 0.01f);
                    }
                }
            }
        }

        private void InForeach(EntitiesHashMap.MyData other, EntitiesHashMap.MyData me, ref float3 avoidanceForce, ref CollisionParameters collision)
        {
            var direction = me.position - other.position;
            var length = math.length(direction);
            if (length > 0.01f)
            {
                var distance = math.max(0f, me.data.innerRadius + other.data.innerRadius - length);
                var distanceOuter = math.max(0f, me.data.innerRadius + other.data.outerRadius - length);
                if (distance > 0f)
                {
                    avoidanceForce += distance * math.normalizesafe(direction);
                    collision.collided++;
                }
                if (distanceOuter > 0f)
                {
                    collision.near++;
                    if (me.data2.broId != other.data2.broId)
                    {
                        collision.nearOther++;
                    }
                }
            }
        }

        private void ForeachAround(EntitiesHashMap.MyData me, ref float3 correction, ref CollisionParameters collision )
        {
            var position = me.position;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, me, ref correction, ref collision);


            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 1));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 1));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, -1));
            Foreach(key, me, ref correction, ref collision);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, -1));
            Foreach(key, me, ref correction, ref collision);
        }

        private void ForeachAround2(CollisionParameters collision, Translation translation, ref float3 correction)
        {
            var position = translation.Value;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            Foreach(key, collision, translation, ref correction);


            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 1));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 1));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, -1));
            Foreach(key, collision, translation, ref correction);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, -1));
            Foreach(key, collision, translation, ref correction);
        }


        private void Foreach(int key, EntitiesHashMap.MyData me, ref float3 avoidanceForce, ref CollisionParameters collision)
        {
            if (targetMap.TryGetFirstValue(key, out EntitiesHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    InForeach(other, me, ref avoidanceForce, ref collision);
                } while (targetMap.TryGetNextValue(out other, ref iterator));
            }
        }

        private void Foreach(int key, CollisionParameters collision, Translation translation, ref float3 correction)
        {
            if (colliders.TryGetFirstValue(key, out CollidersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    ColliderResolve(other, collision, translation, ref correction);
                } while (colliders.TryGetNextValue(out other, ref iterator));
            }
        }
    }
}
