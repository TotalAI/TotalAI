using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryCreateOCT", menuName = "Total AI/Output Change Types/Inventory Create", order = 0)]
    public class InventoryCreateOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Create</b>: Creates inventory for the Agent or the Entity Target of specified EntityType.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Create",
                usesEntityType = true,
                entityTypeLabel = "Entity Type To Create",
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(TagType),
                levelTypeLabel = "Add Tag Type",
                usesIntValue = true,
                intLabel = "Prefab Variant Index"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            if (intAmount > 0)
            {
                int num = target.inventoryType.Add(target, outputChange.entityType, outputChange.intValue, null, intAmount);
                if (num != intAmount)
                    Debug.LogError(target.name + ": InventoryCreateOCT only created " + num + "/" + intAmount + " of " + outputChange.entityType.name);

                if (outputChange.levelType is TagType tagType)
                {
                    // TODO: This isn't right - need to get the actual inventory enties created from Add above
                    Entity entity = target.inventoryType.GetFirstEntityofEntityType(target, outputChange.entityType);
                    entity.AddTag(tagType, target);
                }
            }
            else if (intAmount < 0)
            {
                Debug.LogError("InventoryCreateOCT has a negative amount to take.  Amount To Create must be positive.");
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
            return amount * outputChange.entityType.sideEffectValue;
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
