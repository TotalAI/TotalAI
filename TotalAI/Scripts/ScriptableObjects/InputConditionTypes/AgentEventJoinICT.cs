using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentEventJoinICT", menuName = "Total AI/Input Condition Types/Agent Event Join", order = 0)]
    public class AgentEventJoinICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Agent Event Join</b>: Is this Agent near an Agent Event Type that they can join?",
                usesTypeGroup = true
            };
        }
        
        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (agent.inAgentEvent != null)
                return false;

            // Event can get destroyed right before this check
            if (target == null)
                return false;

            AgentEvent targetAgentEvent = (AgentEvent)target;

            // See if the creator's rLevel for this agent meets the min r requirement for this agent event
            if (targetAgentEvent != null && targetAgentEvent.CanJoin(agent))
            {
                return true;
            }
            return false;
        }
    }
}
