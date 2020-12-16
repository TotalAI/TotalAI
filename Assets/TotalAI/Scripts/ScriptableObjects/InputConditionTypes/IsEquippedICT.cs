using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "IsEquippedICT", menuName = "Total AI/Input Condition Types/Is Equipped", order = 0)]
    public class IsEquippedICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Is Equipped</b>: Does agent have an Inventory Target Match that is equipped or not equipped?",
                usesInventoryTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Is Equipped?"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            List<EntityType> possibleEntityTypes = inputCondition.GetInventoryTypeGroup().GetMatches(agent.inventoryType.GetAllEntityTypes(agent));
            foreach (EntityType entityType in possibleEntityTypes)
            {
                bool equipped = agent.inventoryType.IsEquipped(agent, entityType);
                if ((inputCondition.boolValue && equipped) || (!inputCondition.boolValue && !equipped))
                    return true;
            }
            return false;
        }
    }
}
