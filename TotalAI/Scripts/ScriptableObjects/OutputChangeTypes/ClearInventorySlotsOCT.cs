using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ClearInventorySlotsOCT", menuName = "Total AI/Output Change Types/Clear Inventory Slots", order = 0)]
    public class ClearInventorySlotsOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Clears Inventory Slots</b>: Allows an agent to clear out needed inventory slots. " +
                              "<b>Only use in chain to fix CanAddInventoryICT</b>",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            List<InventorySlot> inventorySlots = target.entityType.canBeInInventorySlots.Select(x => x.inventorySlot).ToList();

            // TODO: Just drop it for now - do better logic later
            List<InventorySlot> possibleSlots = inventorySlots.Intersect(agent.agentType.inventorySlots).ToList();

            if (possibleSlots.Count == 0)
            {
                Debug.LogError(agent.name + ": ClearInventorySlotsOCT - unable to find a slot to clear.");
                return false;
            }

            // This will do nothing if the target doesn't have all of the amount
            // TODO: Add in options to handle not enough inventory
            InventoryType.Item item = target.inventoryType.Remove(target, possibleSlots[0]);
            if (item == null)
                return false;

            item.entity.inventoryType.Dropped(item.entity, item.inventorySlot);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0;
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
            /*
            List<InputCondition> inputConditionsFromOC = outputChangeMappingType.EntityTypeInputConditions();
            List<EntityType> entityTypes = Grouping.PossibleEntityTypes(inputConditionsFromOC, agent.memoryType.GetKnownEntityTypes(agent));

            if (inputConditionMappingType.AnyEntityTypeMatchesGroupings(entityTypes) &&
                outputChange.boolValue == inputCondition.boolValue)
                return true;
            return false;
            */
        }
    }
}
