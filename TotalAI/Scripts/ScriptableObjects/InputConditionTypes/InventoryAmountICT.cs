using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryAmountICT", menuName = "Total AI/Input Condition Types/Inventory Amount", order = 0)]
    public class InventoryAmountICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Amount</b>: Does the agent have at least a certain amount of an Entity Type in its Inventory?",
                usesInventoryTypeGroup = true,
                usesFloatValue = true,
                floatLabel = "Amount",
                usesBoolValue = true,
                boolLabel = "Search Nested"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            List<EntityType> possibleEntityTypes =
                inputCondition.GetInventoryTypeGroup().GetMatches(agent.inventoryType.GetAllEntityTypes(agent, inputCondition.boolValue));
            foreach (EntityType entityType in possibleEntityTypes)
            {
                if (agent.inventoryType.GetEntityTypeAmount(agent, entityType, null, -1, inputCondition.boolValue) >= inputCondition.floatValue)
                    return true;
            }
            return false;
        }
    }
}
