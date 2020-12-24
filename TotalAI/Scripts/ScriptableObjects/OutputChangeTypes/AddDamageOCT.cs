using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AddDamageOCT", menuName = "Total AI/Output Change Types/Add Damage", order = 0)]
    public class AddDamageOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Add Damage</b>: Adds Damage Points to the World Object Type",
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.ActionSkillCurve },
                floatLabel = "Damage Points",
                actionSkillCurveLabel = "Damage Points From Action Skill"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            ((WorldObject)target).ChangeDamage(actualAmount);

            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.ActionSkillCurve)
                return outputChange.actionSkillCurve.Eval0to100(agent.ActionSkillWithItemModifiers(mapping.mappingType.actionType));
            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                   MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
