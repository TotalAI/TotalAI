using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class OutputChangeType : ScriptableObject
    {
        public abstract bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop);
        public abstract float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping);
        public abstract float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount);

        public abstract bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType);

        public virtual bool PreMatch(OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                     MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            return true;
        }

        // These fields are used to aid in the creating of InputConditions
        public class TypeInfo
        {
            public OutputChange.TargetType[] possibleTargetTypes =
                 { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget, OutputChange.TargetType.ToInventoryTarget,
                   OutputChange.TargetType.NewEntity };
            public OutputChange.Timing[] possibleTimings =
                 { OutputChange.Timing.OnFinish, OutputChange.Timing.OnAnimationEvent, OutputChange.Timing.Repeating,
                   OutputChange.Timing.OnBehaviorInvoke, OutputChange.Timing.BeforeStart, OutputChange.Timing.OnCollisionEnter,
                   OutputChange.Timing.OnTriggerEnter, OutputChange.Timing.AfterGameMinutes, OutputChange.Timing.OnInterrupt,
                   OutputChange.Timing.OnQuitAgentEvent };
            public OutputChange.ValueType[] possibleValueTypes =
                 { OutputChange.ValueType.None, OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                   OutputChange.ValueType.PrevOutputAmount, OutputChange.ValueType.ActionSkillCurve, OutputChange.ValueType.BoolValue,
                   OutputChange.ValueType.StringValue, OutputChange.ValueType.EnumValue, OutputChange.ValueType.UnityObject,
                   OutputChange.ValueType.Selector };

            public bool usesInventoryTypeGroupMatchIndex = false;
            public bool usesEntityType = false;
            public string entityTypeLabel = "Entity Type";
            public Type mostRestrictiveEntityType = typeof(EntityType);
            public bool usesLevelType = false;
            public string levelTypeLabel = "Level Type";
            public Type mostRestrictiveLevelType = typeof(LevelType);
            public bool usesFloatValue = false;
            public string floatLabel = "Float Value";
            public bool usesIntValue = false;
            public string intLabel = "Int Value";
            public bool usesBoolValue = false;
            public string boolLabel = "Bool Value";
            public bool usesStringValue = false;
            public string stringLabel = "String Value";
            public bool usesSelector = false;
            public string selectorLabel = "Selector";
            public Type enumType = null;
            public string actionSkillCurveLabel = "Action Skill Curve Value";
            public Type unityObjectType = typeof(UnityEngine.Object);
            public string unityObjectLabel = "Unity Object";
            public string description = "";
        }

        public TypeInfo typeInfo;

        public void OnEnable()
        {
            typeInfo = new TypeInfo();
        }

        // Editor Methods
        public bool CheckEnabledTargetType(Enum arg)
        {
            foreach (OutputChange.TargetType targetType in typeInfo.possibleTargetTypes)
            {
                if (targetType == (OutputChange.TargetType)arg)
                    return true;
            }
            return false;
        }

        public bool CheckEnabledTiming(Enum arg)
        {
            foreach (OutputChange.Timing timing in typeInfo.possibleTimings)
            {
                if (timing == (OutputChange.Timing)arg)
                    return true;
            }
            return false;
        }

        public bool CheckEnabledValueType(Enum arg)
        {
            foreach (OutputChange.ValueType valueType in typeInfo.possibleValueTypes)
            {
                if (valueType == (OutputChange.ValueType)arg)
                    return true;
            }
            return false;
        }
    }
}
