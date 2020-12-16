using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryTakeOCT", menuName = "Total AI/Output Change Types/Inventory Take", order = 0)]
    public class InventoryTakeOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Take</b>: Removes inventory from the Entity Target and gives it to the Agent.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Take",
                usesInventoryTypeGroupMatchIndex = true,
                usesBoolValue = true,
                boolLabel = "Clear Required Slots"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            EntityType entityType = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType;

            bool canBeAdded = agent.inventoryType.FindInventorySlot(agent, entityType, intAmount) != null;
            if (intAmount > 0 && canBeAdded)
            {
                List<InventoryType.Item> items = target.inventoryType.Remove(target, entityType, intAmount, true);
                if (items != null && items.Count == intAmount)
                {
                    agent.inventoryType.Add(agent, items);
                }
                else
                {
                    Debug.Log(agent.name + ": InventoryTakeOCT - target entity didn't have the items.");
                    return false;
                }
            }
            else if (intAmount < 0)
            {
                Debug.LogError(agent.name + ": InventoryTakeOCT has a negative amount to take.  Amount To Take must be positive.");
                return false;
            }
            else if (!canBeAdded && outputChange.boolValue)
            {
                List<InventoryType.Item> items = target.inventoryType.Remove(target, entityType, intAmount, true);
                foreach (InventoryType.Item item in items)
                {
                    agent.inventoryType.ClearSlotsAndAdd(agent, item.entity);
                }
            }
            else if (!canBeAdded)
            {
                Debug.LogError(agent.name + ": InventoryTakeOCT: Can't add " + intAmount + " of " + entityType.name +
                               " and clear required slots is false");
                return false;
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.OppPrevOutputAmount)
                return -mapping.previousOutputChangeAmount;
            else if (outputChange.valueType == OutputChange.ValueType.PrevOutputAmount)
                return mapping.previousOutputChangeAmount;

            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return amount * mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType.sideEffectValue;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // This matches with InventoryAmountICT
            // Matches the InventoryAmountICT InventoryGroup with this OC's InventoryGroup
            InputCondition outputChangeInputCondition = outputChangeMappingType.inputConditions[outputChange.inventoryTypeGroupMatchIndex];
            List<TypeGroup> groupings = outputChangeInputCondition.InventoryTypeGroups(outputChangeMappingType, out List<int> indexesToSet);

            // Groupings is now the EntityTypes that are possible to be taken
            // Need to use allEntityTypes in world since agent may be trying to get an EntityType this is in inventory
            List<EntityType> entityTypes = TypeGroup.InAllTypeGroups(groupings, agent.totalAIManager.allEntityTypes);

            if (inputCondition.AnyInventoryGroupingMatches(inputConditionMappingType, entityTypes))
                return true;
            return false;
        }
    }
}
