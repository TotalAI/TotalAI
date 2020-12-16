using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryDestroyOCT", menuName = "Total AI/Output Change Types/Inventory Destroy", order = 0)]
    public class InventoryDestroyOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Destroy</b>: Removes inventory from the Agent or Entity Target and destroys it.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Destroy",
                usesInventoryTypeGroupMatchIndex = true
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            if (intAmount > 0)
            {
                if (outputChange.targetType == OutputChange.TargetType.ToSelf)
                {
                    Entity inventoryTarget = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];
                    List<InventoryType.Item> items = agent.inventoryType.Remove(agent, inventoryTarget.entityType, intAmount, true);
                    foreach (InventoryType.Item item in items)
                    {
                        item.entity.DestroySelf(agent);
                    }
                }
                else
                {
                    // Currently InventoryGrouping only looks at agent's inventory so mapping.inventoryTarget won't work
                    // Try to find enough of the Grouping in target's inventory
                    // TODO: I think this was fixed?
                    TypeGroup grouping = mapping.mappingType.inputConditions[outputChange.inventoryTypeGroupMatchIndex].GetInventoryTypeGroup();
                    List<InventoryType.Item> items = target.inventoryType.Remove(target, grouping, intAmount, true);
                    if (items == null)
                    {
                        Debug.LogError(agent.name + " InventoryDestroyOCT on Entity Target " + target.name + " - failed to find items to destroy. " +
                                       "MT = " + mapping.mappingType.name + " - Grouping = " + grouping.name);
                        return false;
                    }
                    foreach (InventoryType.Item item in items)
                    {
                        item.entity.DestroySelf(agent);
                    }
                }
            }
            else if (intAmount <= 0)
            {
                Debug.LogError("InventoryDestroyOCT has a negative amount to take.  Amount To Destroy must be positive.");
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
            // Destroying items so return the negative of the value of the items        
            return -amount * mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType.sideEffectValue;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                          InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            List<InputCondition> inputConditionsFromOC = outputChangeMappingType.EntityTypeInputConditions();
            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(inputConditionsFromOC, agent.memoryType.GetKnownEntityTypes(agent));

            if (inputConditionMappingType.AnyEntityTypeMatchesTypeGroups(entityTypes))
                return true;
            return false;
        }
    }
}
