using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryDropOCT", menuName = "Total AI/Output Change Types/Inventory Drop", order = 0)]
    public class InventoryDropOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Drop</b>: Removes inventory from the Agent or Entity Target and drops it on the ground.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Drop",
                usesInventoryTypeGroupMatchIndex = true
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            if (intAmount > 0)
            {
                // Figure out possible EntityTypes from ICs InventoryGroupings - just pick first one
                List<EntityType> entityTypes;
                if (outputChange.inventoryTypeGroupMatchIndex > -1 && outputChange.inventoryTypeGroupMatchIndex < mapping.mappingType.inputConditions.Count)
                {
                    InputCondition inputCondition = mapping.mappingType.inputConditions[outputChange.inventoryTypeGroupMatchIndex];
                    entityTypes = inputCondition.GetInventoryTypeGroup().GetMatches(target.inventoryType.GetAllEntityTypes(target));
                }
                else if (outputChange.inventoryTypeGroupMatchIndex == -1)
                {
                    entityTypes = TypeGroup.PossibleEntityTypes(mapping.mappingType.inputConditions, agent.inventoryType.GetAllEntityTypes(agent), true);
                }
                else
                {
                    Debug.LogError(agent.name + ": InventoryDropOCT has an invalid value for inventoryGroupingMatchIndex in MT: " +
                                   mapping.mappingType.name);
                    return false;
                }
                
                if (entityTypes == null || entityTypes.Count == 0)
                {
                    Debug.LogError(agent.name + ": InventoryDropOCT was unable to find an EntityType from ICs InventoryGroupings for MT: " +
                                   mapping.mappingType.name);
                    return false;
                }
                // TODO: Add in InventoryTFs to MTs
                EntityType entityType = entityTypes[0];

                // This will do nothing if the target doesn't have all of the amount
                // TODO: Add in options to handle not enough inventory
                List<InventoryType.Item> items = target.inventoryType.Remove(target, entityType, intAmount, true);
                if (items == null)
                    return false;

                foreach (InventoryType.Item item in items)
                {
                    // TODO: If Entity was created as Inventory it would have never gone through initialization
                    //       so it won't have an inventoryType set.  Make sure Entities created in inventory get initialized?
                    //item.entity.inventoryType.Dropped(item.entity, item.inventorySlot);
                    agent.inventoryType.Dropped(item.entity, item.inventorySlot);
                }
            }
            else if (intAmount < 0)
                Debug.LogError("InventoryDropOCT has a negative amount to take.  Amount To Drop must be positive.");
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
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return true;
        }
    }
}
