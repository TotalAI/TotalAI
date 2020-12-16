using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryGiveOCT", menuName = "Total AI/Output Change Types/Inventory Give", order = 0)]
    public class InventoryGiveOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Give</b>: Removes inventory from the Agent and gives it to the Entity Target.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Give",
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
                    entityTypes = inputCondition.GetInventoryTypeGroup().GetMatches(agent.inventoryType.GetAllEntityTypes(agent));
                }
                else if (outputChange.inventoryTypeGroupMatchIndex == -1)
                {
                    entityTypes = TypeGroup.PossibleEntityTypes(mapping.mappingType.inputConditions, agent.inventoryType.GetAllEntityTypes(agent), true);
                }
                else
                {
                    Debug.LogError(agent.name + ": InventoryGiveOCT has an invalid value for inventoryGroupingMatchIndex in MT: " +
                                   mapping.mappingType.name);
                    return false;
                }

                if (entityTypes == null || entityTypes.Count == 0)
                {
                    Debug.LogError(agent.name + ": InventoryGiveOCT was unable to find an EntityType from ICs InventoryGroupings for MT: " +
                                   mapping.mappingType.name);
                    return false;
                }
                // TODO: Add in InventoryTFs to MTs
                EntityType entityType = entityTypes[0];

                List<InventoryType.Item> items = agent.inventoryType.Remove(agent, entityType, intAmount, true);
                int numGiven = target.inventoryType.Add(target, items);

                if (numGiven != intAmount)
                {
                    // TODO: Handle partial gives
                    // Failed to Give items for some reason - give back inventory to agent
                    agent.inventoryType.Add(agent, items);

                    Debug.Log(agent.name + ": InventoryGiveOCT was unable to give all to target.  Tried to give " + intAmount + " - gave " + numGiven);
                    return false;
                }
            }
            else if (intAmount < 0)
            {
                Debug.LogError(agent.name + ": InventoryGiveOCT has a negative amount to take.  Amount To Give must be positive.");
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
            return 0;  //amount * mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType.value;
        }

        public override bool PreMatch(OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                      MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            // Matches OtherInventoryAmountICT
            // This needs to be giving the corrent EntityType (InventoryGroupings) based on what the ICT needs (InventoryGroupings)
            if (outputChange.inventoryTypeGroupMatchIndex == -1)
                return false;

            InputCondition outputChangeInputCondition = outputChangeMappingType.inputConditions[outputChange.inventoryTypeGroupMatchIndex];
            List<TypeGroup> groupings = outputChangeInputCondition.InventoryTypeGroups(outputChangeMappingType, out List<int> indexesToSet);

            // groupings is now the EntityTypes that are possible to be given
            List<EntityType> entityTypes = TypeGroup.InAllTypeGroups(groupings, allEntityTypes);

            // See if any of these EntityTypes matches what the IC of OtherInventoryAmountICT needs
            bool inventoryGroupMatches = inputCondition.AnyInventoryGroupingMatches(inputConditionMappingType, entityTypes);

            // Also need to make sure that possible EntityTypes from both MappingTypes have some overlap
            bool groupMatches = outputChangeMappingType.AnyEntityTypeMatchesTypeGroups(inputConditionMappingType, allEntityTypes);
            
            if (inventoryGroupMatches && groupMatches)
                return true;
            return false;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // Matches OtherInventoryAmountICT
            // This needs to be giving the corrent EntityType (InventoryGroupings) based on what the ICT needs (InventoryGroupings)
            InputCondition outputChangeInputCondition = outputChangeMappingType.inputConditions[outputChange.inventoryTypeGroupMatchIndex];
            List<TypeGroup> groupings = outputChangeInputCondition.InventoryTypeGroups(outputChangeMappingType, out List<int> indexesToSet);

            // groupings is now the EntityTypes that are possible to be given
            List<EntityType> entityTypes = TypeGroup.InAllTypeGroups(groupings, agent.memoryType.GetKnownEntityTypes(agent, true));

            // See if any of these EntityTypes matches what the IC of OtherInventoryAmountICT needs
            if (inputCondition.AnyInventoryGroupingMatches(inputConditionMappingType, entityTypes))
                return true;
            return false;
        }
    }
}
