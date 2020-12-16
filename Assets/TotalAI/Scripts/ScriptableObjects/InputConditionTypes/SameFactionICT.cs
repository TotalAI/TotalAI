using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "SameFactionICT", menuName = "Total AI/Input Condition Types/Same Faction", order = 0)]
    public class SameFactionICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Same Faction</b>: Is target Agent in the same or different faction than this Agent?",
                usesTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Same Faction?"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            Agent targetAgent = (Agent)target;

            if (targetAgent != null && ((inputCondition.boolValue && targetAgent.faction == agent.faction) ||
                                        (!inputCondition.boolValue && targetAgent.faction != agent.faction)))                
                return true;
            return false;
        }
    }
}
