using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryPickupOCT", menuName = "Total AI/Output Change Types/Inventory Pickup", order = 0)]
    public class InventoryPickupOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Pickup</b>: Pickup Entity Target from ground and put in Agent's inventory.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
                floatLabel = "Amount To Pickup",
                usesBoolValue = true,
                boolLabel = "Clear Required Slots"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // Make sure target is still there
            // TODO: Do an in range check - not exactly sure what the range should be for pickup?
            if (target.inEntityInventory != null)
            {
                Debug.Log(agent.name + ": InventoryPickupOCT - Target Entity " + target.name + " is gone.");
                return false;
            }

            InventorySlot inventorySlot;
            if (outputChange.boolValue)
                inventorySlot = agent.inventoryType.ClearSlotsAndAdd(agent, target);
            else
                inventorySlot = agent.inventoryType.Add(agent, target);

            if (inventorySlot == null)
            {
                Debug.LogError(agent.name + ": InventoryPickupOCT unable to find inventorySlot for " + target.name);
                return false;
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return amount * target.entityType.sideEffectValue;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                           InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // Trying to match with ItemAmountICT with this (InventoryPickupOCT)
            // So make sure InventoryGrouping from ItemAmountICT overlaps with the Grouping for InventoryPickupOCT
            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(outputChangeMappingType.inputConditions,
                                                                        agent.memoryType.GetKnownEntityTypes(agent));

            if (inputCondition.GetInventoryTypeGroup().AnyMatches(entityTypes))
                return true;
            return false;
        }
    }
}
