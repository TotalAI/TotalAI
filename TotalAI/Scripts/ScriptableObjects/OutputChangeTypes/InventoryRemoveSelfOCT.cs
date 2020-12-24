using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryRemoveSelfOCT", menuName = "Total AI/Output Change Types/Inventory Remove Self", order = 0)]
    public class InventoryRemoveSelfOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Remove Self</b>: Removes this agent from inventory.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            if (agent.inEntityInventory == null)
            {
                Debug.Log(agent.name + ": InventoryRemoveSelfOCT - Agent is not in any inventory.");
                return false;
            }
            Entity inEntity = agent.inEntityInventory;
            InventoryType.Item item = inEntity.inventoryType.Remove(inEntity, agent);

            if (item == null)
            {
                Debug.LogError(agent.name + ": InventoryRemoveSelfOCT unable to find agent in " + inEntity.name);
                return false;
            }

            agent.inventoryType.Dropped(agent, item.inventorySlot);
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
