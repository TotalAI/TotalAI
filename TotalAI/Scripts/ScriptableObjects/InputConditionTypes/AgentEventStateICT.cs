using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentEventStateICT", menuName = "Total AI/Input Condition Types/Agent Event State", order = 0)]
    public class AgentEventStateICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Agent Event State</b>: Is there an Agent Event Type near the Agent that is in the specified state?",
                usesTypeGroup = true,
                usesEnumValue = true,
                enumType = typeof(AgentEvent.State)
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            AgentEvent agentEvent = target as AgentEvent;
            if (agentEvent.state == (AgentEvent.State)inputCondition.enumValueIndex)
                return true;
            return false;
        }
    }
}
