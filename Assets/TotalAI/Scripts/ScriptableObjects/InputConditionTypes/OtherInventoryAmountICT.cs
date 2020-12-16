using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherInventoryAmountICT", menuName = "Total AI/Input Condition Types/Other Inventory Amount", order = 0)]
    public class OtherInventoryAmountICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Other Inventory Amount</b>: Is there an EntityType near the Agent that has the specified EntityType in inventory?",
                boolLabel = "Ignore Can Add Check?",
                usesTypeGroupFromIndex = true,
                usesInventoryTypeGroup = true,
                usesBoolValue = true
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            // Uses both Grouping (Container) and InventoryGrouping (Item)
            // Does Target have any items that are in the InventoryGrouping
            List<EntityType> possibleEntityTypes = inputCondition.GetInventoryTypeGroup().GetMatches(target.inventoryType.GetAllEntityTypes(target));

            foreach (EntityType entityType in possibleEntityTypes)
            {
                bool canAdd = true;
                if (!inputCondition.boolValue)
                {
                    canAdd = agent.inventoryType.FindInventorySlot(agent, entityType) != null;
                }

                if (canAdd && target.inventoryType.GetEntityTypeAmount(target, entityType) > 0)
                    return true;
            }
            return false;
        }
    }
}
