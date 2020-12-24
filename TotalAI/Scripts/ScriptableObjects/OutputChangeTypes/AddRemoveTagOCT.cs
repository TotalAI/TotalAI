using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AddRemoveTagOCT", menuName = "Total AI/Output Change Types/Add Remove Tag", order = 0)]
    public class AddRemoveTagOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Add Remove Tag</b>: Adds or Removes the Tag Type from the Input Entity.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.BoolValue },
                boolLabel = "Add Tag?"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            if (actualAmount > 0)
                target.AddTag((TagType)outputChange.levelType, agent);
            else
                target.RemoveTag((TagType)outputChange.levelType, null);
            
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return outputChange.boolValue ? 1f : -1f;
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
                inputCondition.levelType == outputChange.levelType)
            {
                // Same EntityType and same TagType - match adding tag to needing tag and removing tag to can't have tag
                if (!outputChange.boolValue &&
                    ((HasTagICT.Scope)inputCondition.enumValueIndex == HasTagICT.Scope.DoesNotHaveOwnerTag ||
                     (HasTagICT.Scope)inputCondition.enumValueIndex == HasTagICT.Scope.DoesNotHaveTag))
                    return true;
                else if (outputChange.boolValue &&
                        ((HasTagICT.Scope)inputCondition.enumValueIndex == HasTagICT.Scope.HasTagButNotOwner ||
                         (HasTagICT.Scope)inputCondition.enumValueIndex == HasTagICT.Scope.HasTagIgnoreOwnership ||
                         (HasTagICT.Scope)inputCondition.enumValueIndex == HasTagICT.Scope.HasTagIsOwner))
                    return true;
            }
            return false;
        }
    }
}
