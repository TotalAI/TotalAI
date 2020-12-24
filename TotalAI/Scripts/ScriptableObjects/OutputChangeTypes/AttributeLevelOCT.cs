using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AttributeLevelOCT", menuName = "Total AI/Output Change Types/Attribute Level", order = 0)]
    public class AttributeLevelOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Attribute Level</b>: Change the attribute level of this agent or target agent.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.ActionSkillCurve,
                                                                    OutputChange.ValueType.Selector },
                floatLabel = "Attribute Level Change",
                actionSkillCurveLabel = "Attribute Level Change From Action Skill",
                usesLevelType = true,
                levelTypeLabel = "Attribute Type",
                mostRestrictiveLevelType = typeof(AttributeType),
                usesBoolValue = true,
                boolLabel = "Negate Value"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            float actualChange = ((Agent)target).attributes[(AttributeType)outputChange.levelType].ChangeLevelRelative(actualAmount);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            float amount = outputChange.floatValue;
            if (outputChange.valueType == OutputChange.ValueType.ActionSkillCurve)
                amount = outputChange.actionSkillCurve.Eval0to100(agent.ActionSkillWithItemModifiers(mapping.mappingType.actionType));
            else if (outputChange.valueType == OutputChange.ValueType.Selector)
                amount = outputChange.selector.GetFloatValue(agent, mapping);

            if (outputChange.boolValue)
                return -amount;
            return amount;
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
