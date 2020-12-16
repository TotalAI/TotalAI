using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AddCompleteOCT", menuName = "Total AI/Output Change Types/Add Complete", order = 0)]
    public class AddCompleteOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Add Complete</b>: Add Complete Points to the World Object Type.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.ActionSkillCurve,
                    OutputChange.ValueType.Selector },
                floatLabel = "Complete Points",
                actionSkillCurveLabel = "Complete Points From Action Skill"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            ((WorldObject)target).IncreaseCompletion(actualAmount);
            
            // Add target to parent
            // TODO - might want to expand this to all categories not just CreateEvent
            // This was added due to AgentEvents depending on WorldObjects being completed
            if (mapping.parent != null && mapping.parent.mappingType.inputConditions.Count > 0 &&
                mapping.parent.mappingType.inputConditions[0].inputConditionType.name == "CreateEvent")
                mapping.parent.target = target;

            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.ActionSkillCurve)
                return outputChange.actionSkillCurve.Eval0to100(agent.ActionSkillWithItemModifiers(mapping.mappingType.actionType));
            else if (outputChange.valueType == OutputChange.ValueType.Selector)
                return outputChange.selector.GetFloatValue(agent, mapping);
            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
