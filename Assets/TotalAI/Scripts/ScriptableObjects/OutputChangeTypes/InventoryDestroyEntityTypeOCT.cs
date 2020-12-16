using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryDestroyEntityTypeOCT", menuName = "Total AI/Output Change Types/Inventory Destroy Entity Type", order = 0)]
    public class InventoryDestroyEntityTypeOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Destroy Entity Type</b>: Removes inventory of Entity Type from the Agent or Entity Target and destroys it.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Destroy",
                usesEntityType = true,
                entityTypeLabel = "Entity Type To Destroy",
                usesBoolValue = true,
                boolLabel = "Include Nested Inventory"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            if (intAmount > 0)
            {
                List<InventoryType.Item> items = target.inventoryType.Remove(target, outputChange.entityType, intAmount, true);
                if (items == null || items.Count < intAmount)
                {
                    Debug.LogError("InventoryDestroyEntityTypeOCT was unable to destroy EntityTypes (" + outputChange.entityType.name +
                                   "). Only found " + (items == null ? "0" : items.Count.ToString()) + " out of " + intAmount + ".");
                    return false;
                }
                foreach (InventoryType.Item item in items)
                {
                    item.entity.DestroySelf(agent);
                }
                
            }
            else if (intAmount <= 0)
            {
                Debug.LogError("InventoryDestroyEntityTypeOCT has a negative amount to take.  Amount To Destroy must be positive.");
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
