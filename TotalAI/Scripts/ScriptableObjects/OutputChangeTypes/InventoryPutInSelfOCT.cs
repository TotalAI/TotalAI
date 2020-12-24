using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryPutInSelfOCT", menuName = "Total AI/Output Change Types/Inventory Put In Self", order = 0)]
    public class InventoryPutInSelfOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Put In Self</b>: Places this agent into the inventory of the target.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
                usesBoolValue = true,
                boolLabel = "Clear Target Inv"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // Make sure target is still there
            // TODO: Do an in range check - not exactly sure what the range should be for getting into inventory?
            if (target.inEntityInventory != null)
            {
                Debug.Log(agent.name + ": InventoryPutInSelfOCT - Target Entity " + target.name + " is gone.");
                return false;
            }

            InventorySlot inventorySlot;
            if (outputChange.boolValue)
                inventorySlot = target.inventoryType.ClearSlotsAndAdd(target, agent);
            else
                inventorySlot = target.inventoryType.Add(target, agent);

            if (inventorySlot == null)
            {
                Debug.Log(agent.name + ": InventoryPutInSelfOCT unable to find inventorySlot in " + target.name);
                return false;
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 1f;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0f;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                           InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
