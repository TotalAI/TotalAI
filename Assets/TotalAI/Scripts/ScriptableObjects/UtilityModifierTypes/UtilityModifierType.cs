using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class UtilityModifierType : ScriptableObject
    {
        public abstract float Evaluate(UtilityModifier utilityModifer, Agent agent, Mapping mapping, out bool veto);

        public class TypeInfo
        {
            public string description = "";
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
            public bool usesMinMaxCurve = false;
            public string minMaxCurveLabel = "Min Max Curve";
            public bool usesUnityObject = false;
            public Type unityObjectType = typeof(UnityEngine.Object);
            public string unityObjectLabel = "Unity Object";
        }

        public TypeInfo typeInfo;

        public void OnEnable()
        {
            typeInfo = new TypeInfo();
        }

    }
}