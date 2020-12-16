using System;
using UnityEngine;

namespace TotalAI
{
    public abstract class ChangeConditionType : ScriptableObject
    {
        public abstract bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount);

        public class TypeInfo
        {
            public ChangeCondition.ValueType[] possibleValueTypes =
                 { ChangeCondition.ValueType.None, ChangeCondition.ValueType.FloatValue, ChangeCondition.ValueType.IntValue,
                   ChangeCondition.ValueType.ActionSkillCurve, ChangeCondition.ValueType.BoolValue, ChangeCondition.ValueType.StringValue,
                   ChangeCondition.ValueType.UnityObject, ChangeCondition.ValueType.Selector };

            public bool usesEntityType = false;
            public string entityTypeLabel = "Entity Type";
            public Type mostRestrictiveEntityType = typeof(EntityType);
            public bool usesLevelType = false;
            public string levelTypeLabel = "Level Type";
            public Type mostRestrictiveLevelType = typeof(LevelType);
            public bool usesFloatValue = false;
            public string floatLabel = "Float Value";
            public bool usesBoolValue = false;
            public string boolLabel = "Bool Value";
            public bool usesIntValue = false;
            public string intLabel = "Int Value";
            public bool usesStringValue = false;
            public string stringLabel = "String Value";
            public bool usesSelector = false;
            public string selectorLabel = "Selector";
            public Type enumType = null;
            public string actionSkillCurveLabel = "Action Skill Curve Value";
            public Type unityObjectType = typeof(UnityEngine.Object);
            public string unityObjectLabel = "Unity Object";
            public string attributeValueSelectorLabel = "Attribute Value Selector";
            public string description = "";
            public string outputChangeTypeTooltip = "";
        }

        public TypeInfo typeInfo;

        public void OnEnable()
        {
            typeInfo = new TypeInfo();
        }

        // Editor Method
        public bool CheckEnabledValueType(Enum arg)
        {
            foreach (ChangeCondition.ValueType valueType in typeInfo.possibleValueTypes)
            {
                if (valueType == (ChangeCondition.ValueType)arg)
                    return true;
            }
            return false;
        }
    }
}
