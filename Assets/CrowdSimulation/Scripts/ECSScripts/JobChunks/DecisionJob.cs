using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Assets.CrowdSimulation.Scripts.ECSScripts.ComponentDatas;
using Unity.Transforms;

namespace Assets.CrowdSimulation.Scripts.ECSScripts.JobChunks
{
    [BurstCompile]
    public struct DecisionJobChunk : IJobChunk
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
            var hasGroup = chunk.Has(GroupConditionType);
            var hasCondition = chunk.Has(ConditionType);

            if (!hasGroup && !hasCondition)
            {
                var pathFindings = chunk.GetNativeArray(PathFindingType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var pathfinding = pathFindings[i];
                    pathfinding.lastMessage = DebugMessage.NothingFound;
                    pathFindings[i] = pathfinding;
                }
                return;
            }

            if (!hasCondition)
            {
                HasOnlyGroupCondition(chunk);
                return;
            }

            if (!hasGroup)
            {
                HasOnlyCondition(chunk);
                return;
            }

            HasAllChank(chunk);
        }

        private void HasOnlyGroupCondition(ArchetypeChunk chunk)
        {
            var congroupConditions = chunk.GetNativeArray(GroupConditionType);
            var pathFindings = chunk.GetNativeArray(PathFindingType);
            var translations = chunk.GetNativeArray(TranslationType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var groupCondition = congroupConditions[i];
                var pathfinding = pathFindings[i];
                var translation = translations[i];
                pathfinding.lastMessage = DebugMessage.OnlyGroup;

                if (groupCondition.isSet)
                {
                    pathfinding.decidedGoal = groupCondition.goal;
                    pathfinding.radius = groupCondition.goalRadius;

                } else
                {
                    SetToStop(ref pathfinding, translation);
                }
                pathFindings[i] = pathfinding;
            }
        }

        private void HasOnlyCondition(ArchetypeChunk chunk)
        {
            var conditions = chunk.GetNativeArray(ConditionType);
            var pathFindings = chunk.GetNativeArray(PathFindingType);
            var translations = chunk.GetNativeArray(TranslationType);

            for (var i = 0; i < chunk.Count; i++)
            {
                var condition = conditions[i];
                var pathfinding = pathFindings[i];
                var translation = translations[i];
                pathfinding.lastMessage = DebugMessage.OnlyCondition;

                if (condition.isSet)
                {
                    pathfinding.decidedGoal = condition.goal;
                    pathfinding.radius = 0;
                }
                else
                {
                    SetToStop(ref pathfinding, translation);
                }

                pathFindings[i] = pathfinding;
            }
        }

        private void HasAllChank(ArchetypeChunk chunk)
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
                pathfinding.lastMessage = DebugMessage.All;

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
                        SetToStop(ref pathfinding, translation);
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

            switch(pathFindingData.decisionMethod)
            {
                case DecisionMethod.Max:
                    if (conditionDistance < groupDistance)
                        return DecidedGoalType.Group;
                    else
                        return DecidedGoalType.Condition;
                case DecisionMethod.Min:
                    if (conditionDistance < groupDistance)
                        return DecidedGoalType.Condition;
                    else
                        return DecidedGoalType.Group;
                case DecisionMethod.ConditionOverGroup:
                    return DecidedGoalType.Condition;
                case DecisionMethod.GroupOverCondition:
                    return DecidedGoalType.Group;
            }
            return DecidedGoalType.None;
        }

        private void SetToStop(ref PathFindingData pathFinding, Translation translation)
        {
            pathFinding.decidedGoal = translation.Value;
            pathFinding.radius = 1f;
        }
    }
}
