using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "SetAnimatorParamOCT", menuName = "Total AI/Output Change Types/Set Animator Param", order = 0)]
    public class SetAnimatorParamOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Set Animator Param</b>: Sets the string value or Selector parameter name to the selected value.  " +
                              "If Selector is set OutputChange will use it and not the string value.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget},
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None, OutputChange.ValueType.FloatValue,
                    OutputChange.ValueType.OppPrevOutputAmount, OutputChange.ValueType.PrevOutputAmount, OutputChange.ValueType.BoolValue },
                usesStringValue = true,
                stringLabel = "String Parameter Name",
                usesSelector = true,
                selectorLabel = "Selector Parameter Name",
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // TODO: Use animationType if agent?
            Animator animator = target.GetComponentInChildren<Animator>();

            // TODO: Feels like checks like these should be done once at start of game - InitCheck?
            if (animator == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - SetAnimatorParamOCT: target " + target.name +" has no animator.");
                return false;
            }
            bool selectorValid = outputChange.selector.IsValid();
            if (outputChange.stringValue == "" && !selectorValid)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - SetAnimatorParamOCT: Animator Param name " +
                               "String Value is blank and Selector is not valid.  One of them must be set.");
                return false;
            }

            string paramName = outputChange.stringValue;
            if (selectorValid)
            {
                paramName = outputChange.selector.GetEnumValue<string>(agent, mapping);
            }

            switch (outputChange.valueType)
            {
                case OutputChange.ValueType.None:
                    animator.SetTrigger(paramName);
                    break;
                case OutputChange.ValueType.OppPrevOutputAmount:
                    animator.SetFloat(paramName, -mapping.previousOutputChangeAmount);
                    break;
                case OutputChange.ValueType.PrevOutputAmount:
                    animator.SetFloat(paramName, mapping.previousOutputChangeAmount);
                    break;
                case OutputChange.ValueType.BoolValue:
                    animator.SetBool(paramName, outputChange.boolValue);
                    break;
                case OutputChange.ValueType.FloatValue:
                    animator.SetFloat(paramName, outputChange.floatValue);
                    break;
                default:
                    break;
            }

            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            // Destroying items so return the negative of the value of the items        
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
