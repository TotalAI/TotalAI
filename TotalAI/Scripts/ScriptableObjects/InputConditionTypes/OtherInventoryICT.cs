using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherInventoryICT", menuName = "Total AI/Input Condition Types/Other Inventory", order = 0)]
    public class OtherInventoryICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Other Inventory</b>: Does the entity target have at least a certain amount of an Entity Type in its Inventory?",
                usesInventoryTypeGroup = true,
                usesFloatValue = true,
                usesTypeGroupFromIndex = true,
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            List<EntityType> possibleEntityTypes = inputCondition.GetInventoryTypeGroup().GetMatches(target.inventoryType.GetAllEntityTypes(target));

            foreach (EntityType entityType in possibleEntityTypes)
            {
                if (target.inventoryType.GetEntityTypeAmount(target, entityType) > 0)
                    return true;
            }
            return false;
        }
    }
}
