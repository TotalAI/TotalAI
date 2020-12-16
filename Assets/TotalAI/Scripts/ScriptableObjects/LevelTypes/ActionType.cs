using UnityEngine;
using System.Collections.Generic;
using System;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ActionType", menuName = "Total AI/Level Types/Action Type", order = 0)]
    public class ActionType : LevelType
    {
        [Header("Interaction Distance - If > 0 Overrides Max Distance As Input in the EntityType")]
        public float maxDistanceAsInput;  // This overrides value in the EntityType for max distance to be used as an input
        [Header("How large a search radius for possible Entity Targets?  -1 ignores distance")]
        public float entityTargetSearchRadius = -1f;

        public enum InterruptType { Never, OnlySpecifiedDrives, OnlyDrivesThatCanInterrupt, Always };
        [Header("When can this action be interrupted?")]
        public InterruptType interruptType;
        [Header("How often in realtime seconds to try to interrupt?")]
        public float replanFrequency;
        [Header("How much greater does the new plan's utility need to be to interrupt?")]
        [Tooltip("Set this to a large negative number for this to be ignored.")]
        public float minGreaterUtilityToInterrupt;
        [Header("Which Drive Types should be able to interrupt?")]
        public List<DriveType> interruptingDriveTypes;

        [Header("Forces Agent to immediately call Decider.Run to avoid waiting")]
        [Tooltip("If this Action Type is interrupted, force immediate replanning to start next plan.")]
        public bool noWaitOnInterrupt;
        [Tooltip("If this Action Type is finished and the plan is finished, force immediate replanning to start next plan.")]
        public bool noWaitOnFinishNoNextMapping;
        [Tooltip("If this Action Type is finished and there is a next mapping in the plan, force next mapping to start immediately.")]
        public bool noWaitOnFinishHasNextMapping;

        [Header("Which Inventory Slots should be considered used for this Action Type?")]
        public List<InventorySlot> usesInventorySlots;

        [Header("What code will run for this Action Type?")]
        public BehaviorType behaviorType;

        // This list is generated from MappingTypes which have an ActionType on them
        public List<MappingType> mappingTypes;

        public List<MappingType> FilteredMappingTypes(AgentType agentType)
        {
            List<MappingType> filtered = new List<MappingType>();
            foreach (MappingType mappingType in mappingTypes)
            {
                if (mappingType.AllowedByAgentType(agentType))
                    filtered.Add(mappingType);
            }
            return filtered;
        }

        // Can the passed in agent perform this ActionType - has all the ICs for at least one MappingType attached to this ActionType
        // For Deep Learning
        /*
        public bool CanPerform(Agent agent)
        {
            Mapping mapping = new Mapping(this, null);

            foreach (MappingType mappingType in mappingTypes)
            {
                mapping.mappingType = mappingType;

                bool passedAllChecks = true;
                foreach (InputCondition inputCondition in mappingType.inputConditions)
                {
                    if (!inputCondition.Check(agent, mapping, false))
                    {
                        passedAllChecks = false;
                        break;
                    }
                }

                if (passedAllChecks)
                    return true;
            }

            return false;
        }
        */
        
        public override string ToString()
        {
            return name + " - " + behaviorType.name + " (" + mappingTypes.Count + ")";
        }
    }
}



