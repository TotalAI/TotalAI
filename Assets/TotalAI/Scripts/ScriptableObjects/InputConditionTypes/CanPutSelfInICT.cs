using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "CanPutSelfInICT", menuName = "Total AI/Input Condition Types/Can Put Self In", order = 0)]
    public class CanPutSelfInICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Can Put Self In</b>: Does the Target Entity have a spot for this agent?",
                usesTypeGroup = true
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            return target.inventoryType.FindInventorySlot(target, agent.entityType) != null;
        }
    }
}
