﻿using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct FighterJob : IJobChunk 
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

        public ComponentTypeHandle<Fighter> FighterHandle;
        public ComponentTypeHandle<PathFindingData> PathFindingDataHandle;
        [ReadOnly] public ComponentTypeHandle<Translation> TranslationHandle;
        public ComponentTypeHandle<Rotation> RotationHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var fighters = chunk.GetNativeArray(FighterHandle);
            var pathfindings = chunk.GetNativeArray(PathFindingDataHandle);
            var rotations = chunk.GetNativeArray(RotationHandle);
            var translations = chunk.GetNativeArray(TranslationHandle);

            for (var i = 0; i < chunk.Count; i++)
            {
                var fighter = fighters[i];
                var pathfinding = pathfindings[i];
                var rotation = rotations[i];
                var translation = translations[i];

                Execute(ref fighter, ref pathfinding, ref translation, ref rotation);

                fighters[i] = fighter;
                rotations[i] = rotation;
                pathfindings[i] = pathfinding;
            }
        }


        public void Execute(ref Fighter fighter, ref PathFindingData pathFindingData, 
            [ReadOnly] ref Translation translation, ref Rotation rotation)
        {
            var selected = new FightersHashMap.MyData();
            var found = ForeachAround(translation.Value, ref selected, fighter);
            if (found)
            {
                var direction = selected.position - translation.Value;
                fighter.targetId = selected.data.Id;
                pathFindingData.radius = fighter.attackRadius * 0.5f;
                if (math.length(direction) < fighter.attackRadius)
                {
                    pathFindingData.decidedGoal = translation.Value + direction * 0.01f;
                    RotateForward(direction, ref rotation);
                    fighter.state = FightState.Fight;
                } else
                {
                    pathFindingData.decidedGoal = translation.Value + direction * 0.5f;
                    fighter.state = FightState.GoToFight;
                }
            }
            else
            {
                fighter.targetId = -1;
                var isNear = GetNear(fighter.goalPos, fighter.goalRadius, translation, ref pathFindingData);
                fighter.state = isNear ? FightState.Standing : FightState.GoToPlace;
            }
        }

        private void RotateForward(float3 direction, ref Rotation rotation)
        {
            var speed = math.length(direction);

            if (speed > 0.01f)
            {
                var toward = quaternion.LookRotationSafe(direction, new float3(0, 1, 0));
                rotation.Value = toward;
            }
        }

        private bool GetNear(float3 goal, float radius, Translation translation, ref PathFindingData pathFindingData)
        {
            var force = (goal - translation.Value);
            pathFindingData.radius = radius;
            if (math.length(force) > radius)
            {
                pathFindingData.decidedGoal = goal;
                return false;
            }
            else
            {
                pathFindingData.decidedGoal = translation.Value;
                return true;
            }
        }

        private bool ForeachAround(float3 position, ref FightersHashMap.MyData output, Fighter myBroId)
        {
            var found = false;
            var key = QuadrantVariables.GetPositionHashMapKey(position);
            found = found || Foreach(key, position, ref output, found, myBroId);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(1, 0, 0));
            found = found || Foreach(key, position, ref output, found, myBroId);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(-1, 0, 0));
            found = found || Foreach(key, position, ref output, found, myBroId);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, 1));
            found = found || Foreach(key, position, ref output, found, myBroId);
            key = QuadrantVariables.GetPositionHashMapKey(position, new float3(0, 0, -1));
            found = found || Foreach(key, position, ref output, found, myBroId);
            return found;
        }

        private bool Foreach(int key, float3 position, ref FightersHashMap.MyData output, bool found, Fighter me)
        {
            if (targetMap.TryGetFirstValue(key, out FightersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (other.data.groupId == me.groupId) continue;
                    var nowDistance = math.length(other.position - position);
                    if (!found && nowDistance < me.viewRadius)
                    {
                        output = other;
                        found = true;
                    }
                    else
                    {
                        var prevDist = math.length(output.position - position);
                        if (prevDist > nowDistance)
                        {
                            output = other;
                        }
                    }

                } while (targetMap.TryGetNextValue(out other, ref iterator));
            }
            return found;
        }
    }
}
