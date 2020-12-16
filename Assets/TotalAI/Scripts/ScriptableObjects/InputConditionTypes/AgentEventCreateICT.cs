using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentEventCreateICT", menuName = "Total AI/Input Condition Types/Agent Event Create", order = 0)]
    public class AgentEventCreateICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Create Agent Event</b>: Can this agent create this Agent Event Type?",
                usesEntityType = true,
                mostRestrictiveEntityType = typeof(AgentEventType)
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            // Agent needs permission to create this event based on his or her faction role
            if (((AgentEventType)inputCondition.entityType).CanCreate(agent))
                return true;
            return false;
        }
    }
}
