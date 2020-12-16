using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MappingType", menuName = "Total AI/Mapping Type", order = 1)]
    public class MappingType : ScriptableObject
    {
        [Header("What Action Type is performed to do this Mapping Type?")]
        public ActionType actionType;

        [Header("What Action Type to use for going to the Target Entity?")]
        public ActionType goToActionType;

        public enum FilterOptions { Neither, UseBlacklist, UseWhitelist }
        [Header("Overrides AgentType being able to use Mapping Type if agent has the Action Type")]
        public FilterOptions filterOptions;
        public List<AgentType> blacklistAgentTypes;
        public List<AgentType> whitelistAgentTypes;

        public List<bool> overrideDefaultSelectors;
        public List<Selector> selectors;
        public List<bool> overrideDefaultGoToSelectors;
        public List<Selector> goToSelectors;

        [Header("Overrides value in the ActionType and EntityType")]
        public float maxDistanceAsInput;

        [Header("Radius to search for Entity Targets - 0 to use ActionType's searchRadius")]
        public float entityTargetSearchRadius;

        [Header("Before running this reevaluate all targets and inventory targets in the plan")]
        public bool reevaluateTargets;

        [Serializable]
        public class UtilityModifierInfo
        {
            public UtilityModifier utilityModifier;
            public float weight;
        }

        public List<UtilityModifierInfo> utilityModifierInfos;

        [Serializable]
        public class TargetFactorInfo
        {
            public TargetFactor targetFactor;
            public float weight;
        }

        public List<TargetFactorInfo> targetFactorInfos;
        public List<TargetFactorInfo> inventoryFactorInfos;

        public List<InputCondition> inputConditions;
        public List<OutputChange> outputChanges;

        private void OnEnable()
        {
            // Generate new TypeGroups for InputConditions
            if (inputConditions != null)
            {
                foreach (InputCondition inputCondition in inputConditions)
                {
                    inputCondition.GenerateTypeGroup();
                }
            }
        }

        // TODO: These could be cached
        public List<Selector> GetAttributeSelectors()
        {
            List<Selector> results = new List<Selector>();
            for (int i = 0; i < selectors.Count; i++)
            {
                if (overrideDefaultSelectors[i])
                    results.Add(selectors[i]);
                else
                    results.Add(actionType.behaviorType.defaultSelectors[i]);
            }
            return results;
        }

        // Returns the index of the first OutputChangeType that matches the InputConditionType or -1 if no matches
        public int OutputChangeTypeMatch(InputCondition inputCondition, MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            for (int i = 0; i < outputChanges.Count; i++)
            {
                OutputChange outputChange = outputChanges[i];

                if (inputCondition.inputConditionType == null)
                {
                    Debug.LogError("MappingType " + inputConditionMappingType.name + " has an InputCondition with no InputConditionType.  Please Fix.");
                    return -1;
                }
                if (outputChange.outputChangeType == null)
                {
                    Debug.LogError("MappingType " + name + " has an OutputChange with no OutputChangeType.  Please Fix.");
                    return -1;
                }
                if (inputCondition.inputConditionType.matchingOCTs != null &&
                    inputCondition.inputConditionType.matchingOCTs.Contains(outputChange.outputChangeType) &&
                    outputChange.outputChangeType.PreMatch(outputChange, this, inputCondition, inputConditionMappingType, allEntityTypes))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool AllowedByAgentType(AgentType agentType)
        {
            switch (filterOptions)
            {
                case FilterOptions.UseBlacklist:
                    if (blacklistAgentTypes.Contains(agentType))
                        return false;
                    break;
                case FilterOptions.UseWhitelist:
                    if (!whitelistAgentTypes.Contains(agentType))
                        return false;
                    break;
            }
            return true;
        }
        
        // Returns a list of all items consumed in this MappingType
        /*
        public List<ItemType> ItemsConsumed()
        {
            List<ItemType> consumedItems = new List<ItemType>();
            foreach (OutputChange outputChange in outputChanges)
            {
                if (outputChange.outputChangeType.name == "ItemAmountOCT" && outputChange.floatValue < 0)
                {
                    consumedItems.Add((ItemType)outputChange.inputOutputType);
                }
            }
            return consumedItems;
        }
        */

        public bool ForEntityType()
        {
            if (inputConditions == null || inputConditions.Count == 0)
                return false;
            foreach (InputCondition inputCondition in inputConditions)
            {
                if (inputCondition.RequiresEntityTarget())
                    return true;
            }
            return false;
        }

        public bool HasAnyInventoryTargets()
        {
            if (inputConditions == null || inputConditions.Count == 0)
                return false;
            foreach (InputCondition inputCondition in inputConditions)
            {
                if (inputCondition.RequiresInventoryTarget())
                    return true;
            }
            return false;
        }

        public List<InputCondition> EntityTypeInputConditions()
        {
            if (inputConditions == null || inputConditions.Count == 0)
                return null;

            List<InputCondition> results = new List<InputCondition>();
            foreach (InputCondition inputCondition in inputConditions)
            {
                if (inputCondition.RequiresEntityTarget())
                    results.Add(inputCondition);
            }
            return results;
        }

        public List<InputCondition> NonEntityTypeInputConditions()
        {
            List<InputCondition> results = new List<InputCondition>();
            if (inputConditions != null)
            {
                foreach (InputCondition inputCondition in inputConditions)
                {
                    if (!inputCondition.RequiresEntityTarget())
                        results.Add(inputCondition);
                }
            }
            return results;
        }
        
        public List<InputCondition> InventoryTargetInputConditions()
        {
            if (inputConditions == null || inputConditions.Count == 0)
                return null;

            List<InputCondition> results = new List<InputCondition>();
            foreach (InputCondition inputCondition in inputConditions)
            {
                if (inputCondition.RequiresInventoryTarget())
                    results.Add(inputCondition);
            }
            return results;
        }

        public bool HasAnyInputConditions(List<InputCondition> inputConditionsToFind)
        {
            if (inputConditions != null)
            {
                foreach (InputCondition inputCondition in inputConditions)
                {
                    if (inputConditionsToFind.Contains(inputCondition))
                        return true;
                }
            }
            return false;
        }
        
        public List<TypeGroup> EntityTypeGroupings(bool forceTypeGroupRefresh = false)
        {
            if (inputConditions == null)
                return new List<TypeGroup>();
            return inputConditions.Where(x => x.GetTypeGroup(forceTypeGroupRefresh) != null)
                                  .Select(x => x.GetTypeGroup(forceTypeGroupRefresh)).Distinct().ToList();
        }

        public bool AnyEntityTypeMatchesTypeGroups(List<EntityType> entityTypes)
        {
            List<TypeGroup> groupings = EntityTypeGroupings(true);
            if (groupings.Count == 0)
                return false;

            List<EntityType> matches = TypeGroup.InAllTypeGroups(groupings, entityTypes, true);
            return matches != null && matches.Count > 0;
        }

        // Returns true if this and mappingType have any EntityTypes that match both
        public bool AnyEntityTypeMatchesTypeGroups(MappingType mappingType, List<EntityType> entityTypes)
        {
            List<TypeGroup> typeGroups = EntityTypeGroupings(true);
            if (typeGroups.Count == 0)
                return false;

            List<TypeGroup> otherTypeGroups = mappingType.EntityTypeGroupings(true);
            if (otherTypeGroups.Count == 0)
                return false;

            List<TypeGroup> combinedTypeGroups = typeGroups.Union(otherTypeGroups).Distinct().ToList();
            List<EntityType> matches = TypeGroup.InAllTypeGroups(combinedTypeGroups, entityTypes, true);
            return matches != null && matches.Count > 0;
        }

        public float GetEntityTargetSearchRadius()
        {
            if (entityTargetSearchRadius > 0)
                return entityTargetSearchRadius;
            else if (actionType.entityTargetSearchRadius > 0)
                return actionType.entityTargetSearchRadius;

            // TODO: Use a global searchRadius setting or make sure actionType searchRadius is always set
            return -1f;
        }

        public float GetPreviousValue(OutputChange outputChange)
        {
            bool foundOutputChange = false;
            for (int i = outputChanges.Count - 1; i >= 0; i--)
            {
                if (outputChange == outputChanges[i])
                    foundOutputChange = true;
                else if (foundOutputChange && outputChanges[i].valueType == OutputChange.ValueType.FloatValue)
                    return outputChanges[i].floatValue;
            }
            Debug.LogError(name + ": MappingType.GetPreviousValue - Never found a float value!");
            return 0;
        }
        
        public Dictionary<OutputChange, float> InitLastUpdateTimes()
        {
            Dictionary<OutputChange, float> lastUpdates = new Dictionary<OutputChange, float>();
            if (outputChanges != null)
            {
                foreach (OutputChange outputChange in outputChanges)
                {
                    if (outputChange.timing == OutputChange.Timing.Repeating || outputChange.timing == OutputChange.Timing.AfterGameMinutes)
                    {
                        lastUpdates.Add(outputChange, Time.time);
                    }
                }
            }
            return lastUpdates;
        }

        // We want tostring to print out the Inputs -> ActionType -> Ouputs
        public override string ToString()
        {
            string mappingAsString = name;

            if (maxDistanceAsInput != 0)
                mappingAsString += "(dist = " + maxDistanceAsInput + ")";

            mappingAsString += ": ";

            if (inputConditions != null)
            {
                for (int i = 0; i < inputConditions.Count; i++)
                    mappingAsString += inputConditions[i] + " : ";
            }

            if (mappingAsString.Length > 3) mappingAsString = mappingAsString.Substring(0, mappingAsString.Length - 3);
            else mappingAsString = "None";

            mappingAsString += " => ";

            if (outputChanges != null)
            {
                for (int i = 0; i < outputChanges.Count; i++)
                {
                    mappingAsString += outputChanges[i] + " : ";

                }
            }

            if (outputChanges == null || outputChanges.Count == 0) mappingAsString += "None : ";

            if (mappingAsString.Length > 3) mappingAsString = mappingAsString.Substring(0, mappingAsString.Length - 3);

            return mappingAsString;
        }
        
    }
}
