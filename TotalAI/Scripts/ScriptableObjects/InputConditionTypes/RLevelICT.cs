using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RLevelICT", menuName = "Total AI/Input Condition Types/RLevel", order = 0)]
    public class RLevelICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>RLevel</b>: Is Agent's RLevel of the other Agent within the min-max range?",
                usesTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Use Other Agent's Memory",
                usesMinMax = true,
                minLabel = "Min RLevel",
                maxLabel = "Max RLevel",
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (target != null)
            {
                MemoryType.EntityInfo entityInfo = agent.memoryType.KnownEntity(agent, target);
                if (entityInfo != null && entityInfo.rLevel >= inputCondition.min && entityInfo.rLevel <= inputCondition.max)
                    return true;
            }
            return false;
        }
    }
}
