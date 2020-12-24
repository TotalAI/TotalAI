using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class InputConditionType : ScriptableObject
    {
        public abstract bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck);
        public List<OutputChangeType> matchingOCTs;

        // These fields are used to aid in the creating of InputConditions
        public class TypeInfo
        {
            public bool usesTypeGroup = false;
            public bool usesTypeGroupFromIndex = false;
            public bool usesInventoryTypeGroup = false;
            public bool usesInventoryTypeGroupShareWith = false;
            public bool usesEntityType = false;
            public bool usesEntityTypes = false;
            public Type mostRestrictiveEntityType = typeof(EntityType);
            public bool usesLevelType = false;
            public bool usesLevelTypes = false;
            public Type mostRestrictiveLevelType = typeof(LevelType);
            public bool usesFloatValue = false;
            public string floatLabel = "Float Value";
            public bool usesIntValue = false;
            public string intLabel = "Integer Value";
            public bool usesBoolValue = false;
            public string boolLabel = "Bool Value";
            public bool usesStringValue = false;
            public string stringLabel = "String Value";
            public bool usesEnumValue = false;
            public Type enumType = null;
            public bool usesMinMax = false;
            public float minRange = 0;
            public string minLabel = "Min";
            public float maxRange = 100;
            public string maxLabel = "Max";
            public bool usesActionSkillCurve = false;
            public string actionSkillCurveLabel = "";
            public string description = "";
            public string inputConditionTypeTooltip = "";
            public string inputOutputTypeTooltip = "";
        }

        public TypeInfo typeInfo;

        public void OnEnable()
        {
            typeInfo = new TypeInfo();
        }
    }
}