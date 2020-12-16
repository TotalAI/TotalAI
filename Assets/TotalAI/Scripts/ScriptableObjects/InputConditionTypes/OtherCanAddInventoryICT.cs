using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherCanAddInventoryICT", menuName = "Total AI/Input Condition Types/Other Can Add Inventory", order = 0)]
    public class OtherCanAddInventoryICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Other Can Add Inventory</b>: Does the Target Entity have a spot for an EntityType of InventoryGrouping?",
                usesTypeGroupFromIndex = true,
                usesInventoryTypeGroup = true,
                usesInventoryTypeGroupShareWith = true
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            // Look at all of the InventoryGroupings in this MappingType - See what EntityTypes match
            // TODO: Figure out how inventoryTarget gets set
            List<TypeGroup> groupings = inputCondition.InventoryTypeGroups(mapping.mappingType, out List<int> indexesToSet);
            if (groupings == null)
            {
                Debug.LogError(agent.name + ": OtherCanAddInventoryICT missing InventoryGroupings - MT = " + mapping.mappingType);
                return false;
            }
            List<EntityType> possibleEntityTypes = TypeGroup.InAllTypeGroups(groupings, agent.memoryType.GetKnownEntityTypes(agent));
            foreach (EntityType entityType in possibleEntityTypes)
            {
                if (target.inventoryType.FindInventorySlot(target, entityType) != null)
                    return true;
            }
            return false;
        }
    }
}
