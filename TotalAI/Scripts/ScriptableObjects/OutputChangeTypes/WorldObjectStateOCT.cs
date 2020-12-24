using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WorldObjectStateOCT", menuName = "Total AI/Output Change Types/World Object State", order = 0)]
    public class WorldObjectStateOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>World Object State</b>: Change the state to the string name.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToInventoryTarget, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.StringValue, OutputChange.ValueType.Selector },
                stringLabel = "New State",
                selectorLabel = "New State"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;
            WorldObject worldObject = target as WorldObject;

            WorldObjectType.State newState = worldObject.worldObjectType.states.Find(x => x.name == outputChange.stringValue);
            if (newState == null)
            {
                Debug.LogError(agent.name + ": " + worldObject.name + " '" + worldObject.currentState.name + "' WorldObjectStateOCT - Trying to switch" +
                               "to state '" + outputChange.stringValue + "' that doesn't exist.  Please check spelling.");
                return false;
            }

            return ((WorldObject)target).ChangeState(agent, newState);
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            // State change has no amounts - Could states have utility values though?
            return 0f;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // This matches WorldObjectStateICT
            List<InputCondition> inputConditionsFromOC = outputChangeMappingType.EntityTypeInputConditions();
            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(inputConditionsFromOC, agent.memoryType.GetKnownEntityTypes(agent));

            if (inputConditionMappingType.AnyEntityTypeMatchesTypeGroups(entityTypes) &&
                outputChange.stringValue == inputCondition.stringValue)
                return true;
            return false;
        }
    }
}
