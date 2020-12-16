using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TargetedICT", menuName = "Total AI/Input Condition Types/Targeted", order = 0)]
    public class TargetedICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Targeted</b>: Is this Agent the target of Action Type being performed by an Agent Type?",
                usesTypeGroup = true,
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(ActionType),
                usesMinMax = true,
                minLabel = "Min RLevel",
                maxLabel = "Max RLevel",
                usesFloatValue = true,
                floatLabel = "Max Seconds Elapsed In Action"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            Agent otherAgent = target as Agent;
            if (otherAgent == null)
                return false;

            // Condition is satisfied if agent is the target and the other agent is performing the correct action
            if (otherAgent.decider.CurrentMapping != null && otherAgent.decider.CurrentMapping.target == agent &&
                otherAgent.decider.CurrentMapping.mappingType.actionType == inputCondition.levelType &&
                (inputCondition.floatValue <= 0f || Time.time - otherAgent.decider.PlanStartTime < inputCondition.floatValue))
            {
                // Possible to also have an R level requirement - set max to 0 to ignore this check
                if (inputCondition.max == 0)
                {
                    return true;
                }
                else
                {
                    MemoryType.EntityInfo entityInfo = agent.memoryType.KnownEntity(agent, target);
                    if (entityInfo != null && entityInfo.rLevel >= inputCondition.min && entityInfo.rLevel <= inputCondition.max)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
