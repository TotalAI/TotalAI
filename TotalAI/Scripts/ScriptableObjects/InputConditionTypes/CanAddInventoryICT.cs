using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "CanAddInventoryICT", menuName = "Total AI/Input Condition Types/Can Add Inventory", order = 0)]
    public class CanAddInventoryICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Can Add Inventory</b>: Does the Agent have a spot for the target EntityType?",
                usesTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Allow Clearing Spots"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (inputCondition.boolValue)
            {
                agent.inventoryType.FindInventorySlotsToClear(agent, target.entityType, out InventorySlot inventorySlot, 1, true);
                return inventorySlot != null;
            }
            return agent.inventoryType.FindInventorySlot(agent, target.entityType) != null;
        }
    }
}
