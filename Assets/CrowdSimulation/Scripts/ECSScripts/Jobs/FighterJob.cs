using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Assets.CrowdSimulation.Scripts.ECSScripts.Systems;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{
    [BurstCompile]
    public struct FighterJob : IJobForEach<Fighter, Condition, PathFindingData, Translation, Rotation>
    {
        [NativeDisableParallelForRestriction]
        [ReadOnly]
        public NativeMultiHashMap<int, FightersHashMap.MyData> targetMap;

        public void Execute(ref Fighter fighter, ref Condition condition, ref PathFindingData pathFindingData, 
            [ReadOnly] ref Translation translation, ref Rotation walker)
        {
            var selected = new FightersHashMap.MyData();
            var found = ForeachAround(translation.Value, ref selected, fighter.groupId);
            if (found)
            {
                var direction = selected.position - translation.Value;
                fighter.targetId = selected.data.Id;
                pathFindingData.radius = fighter.attackRadius * 0.5f;
                if (math.length(direction) < fighter.attackRadius)
                {
                    pathFindingData.decidedGoal = translation.Value + direction * 0.1f;
                    RotateForward(direction, ref walker);
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

            if (speed > 0.1f)
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

        private bool ForeachAround(float3 position, ref FightersHashMap.MyData output, int myBroId)
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

        private bool Foreach(int key, float3 position, ref FightersHashMap.MyData output, bool found, int myBroId)
        {
            if (targetMap.TryGetFirstValue(key, out FightersHashMap.MyData other, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    if (other.data.groupId == myBroId) continue;
                    if (!found)
                    {
                        output = other;
                        found = true;
                    }
                    else
                    {
                        var prevDist = math.length(output.position - position);
                        var nowDistance = math.length(other.position - position);
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
