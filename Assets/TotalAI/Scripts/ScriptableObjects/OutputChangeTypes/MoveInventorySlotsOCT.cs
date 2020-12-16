using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MoveInventorySlotsOCT", menuName = "Total AI/Output Change Types/Move Inventory Slots", order = 0)]
    public class MoveInventorySlotsOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Move Inventory Slots</b>: Allows an agent to move Entities to a different InventorySlot",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.BoolValue },
                boolLabel = "Equip Item?",
                usesInventoryTypeGroupMatchIndex = true
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            Entity inventoryTargetEntity = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];
            target.inventoryType.MoveInventorySlot(target, inventoryTargetEntity, outputChange.boolValue, true);
            
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
            List<InputCondition> inputConditionsFromOC = outputChangeMappingType.EntityTypeInputConditions();
            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(inputConditionsFromOC, agent.memoryType.GetKnownEntityTypes(agent));

            if (inputConditionMappingType.AnyEntityTypeMatchesTypeGroups(entityTypes) &&
                outputChange.boolValue == inputCondition.boolValue)
                return true;
            return false;
        }
    }
}
