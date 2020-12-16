using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "AttributeLevelOtherICT", menuName = "Total AI/Input Condition Types/Attribute Level Other", order = 0)]
    public class AttributeLevelOtherICT : InputConditionType
    {
        public enum RangeType { GreaterThanOrEqual, LessThanOrEqual, RangeInclusive }

        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Attribute Level Other</b>: Conditions for an Target Agent's Attribute Level.",
                usesTypeGroupFromIndex = true,
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(AttributeType),
                usesEnumValue = true,
                enumType = typeof(RangeType),
                usesFloatValue = true,
                floatLabel = "Value For >= or <=",
                usesMinMax = true,
                minLabel = "Min For Range Inclusive",
                maxLabel = "Max For Range Inclusive"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            float attributeLevel = target.attributes[(AttributeType)inputCondition.levelType].GetLevel();
            RangeType rangeType = (RangeType)inputCondition.enumValueIndex;

            switch (rangeType)
            {
                case RangeType.GreaterThanOrEqual:
                    if (attributeLevel >= inputCondition.floatValue)
                        return true;
                    break;
                case RangeType.LessThanOrEqual:
                    if (attributeLevel <= inputCondition.floatValue)
                        return true;
                    break;
                case RangeType.RangeInclusive:
                    if (attributeLevel >= inputCondition.min && attributeLevel <= inputCondition.max)
                        return true;
                    break;
            }

            return false;
        }
    }
}
