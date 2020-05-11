using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.Jobs
{

    public struct DecisiionJobChunk : IJobChunk
    {

        enum DecidedGoalType
        {
            Group, Condition, None
        }

        [ReadOnly] public ArchetypeChunkComponentType<GroupCondition> GroupConditionType;
        [ReadOnly] public ArchetypeChunkComponentType<Condition> ConditionType;
        public ArchetypeChunkComponentType<PathFindingData> PathFindingType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var groupConditions = chunk.GetNativeArray(GroupConditionType);
            var conditions = chunk.GetNativeArray(ConditionType);
            var pathFindings = chunk.GetNativeArray(PathFindingType);
            var translations = chunk.GetNativeArray(TranslationType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var groupCondition = groupConditions[i];
                var condition = conditions[i];
                var pathfinding = pathFindings[i];
                var translation = translations[i];

                var decidedGoal = DecidedGoal(groupCondition, condition, translation, ref pathfinding);

                switch (decidedGoal)
                {
                    case DecidedGoalType.Condition:
                        pathfinding.decidedGoal = condition.goal;
                        pathfinding.radius = 0;
                        break;
                    case DecidedGoalType.Group:
                        pathfinding.decidedGoal = groupCondition.goal;
                        pathfinding.radius = groupCondition.goalRadius;
                        break;
                    case DecidedGoalType.None:
                        pathfinding.decidedGoal = translation.Value;
                        pathfinding.radius = 0;
                        break;
                }

                pathFindings[i] = pathfinding;
            }
        }

        private DecidedGoalType DecidedGoal(GroupCondition group, Condition condition, Translation translation, ref PathFindingData pathFindingData)
        {

            if (!condition.isSet)
            {
                if (!group.isSet)
                {
                    return DecidedGoalType.None;
                }
                return DecidedGoalType.Group;
            }
            if (!group.isSet)
            {
                return DecidedGoalType.Condition;
            }

            var conditionDistance = math.length(condition.goal - translation.Value);
            var groupDistance = math.length(group.goal - translation.Value);
            if (pathFindingData.decisionMethod == DecisionMethod.Max)
            {
                if (conditionDistance < groupDistance)
                    return DecidedGoalType.Group;
                else
                    return DecidedGoalType.Condition;
            }
            if (pathFindingData.decisionMethod == DecisionMethod.Min)
            {
                if (conditionDistance < groupDistance)
                    return DecidedGoalType.Condition;
                else
                    return DecidedGoalType.Group;
            }
            return DecidedGoalType.None;
        }
    }

    [BurstCompile]
    public struct SetGroupForceJob : IJobForEach<GroupCondition, PathFindingData, Translation>
    {
        public void Execute([ReadOnly] ref GroupCondition group, ref PathFindingData pathFindingData, [ReadOnly]ref Translation translation)
        {
            if (group.isSet)
            {
                pathFindingData.decidedGoal = group.goal;
                pathFindingData.radius = group.goalRadius;
            } else
            {
                pathFindingData.decidedGoal = translation.Value;
            }
        }
    }

    [BurstCompile]
    public struct SetDesireForceJob : IJobForEach<Condition, PathFindingData, Translation>
    {
        public void Execute([ReadOnly] ref Condition condition, ref PathFindingData pathFindingData, [ReadOnly]ref Translation translation)
        {
            if (condition.isSet)
            {
                pathFindingData.decidedGoal = condition.goal;
            } else
            {
                pathFindingData.decidedGoal = translation.Value;
            }
        }
    }
}
