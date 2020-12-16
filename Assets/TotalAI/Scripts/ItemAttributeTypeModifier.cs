using System;
using System.Collections.Generic;

namespace TotalAI
{
    [Serializable]
    public class ItemAttributeTypeModifier
    {
        public ItemCondition itemCondition;

        public enum ModifierType { OnAction, Always }
        public ModifierType modifierType;

        public AttributeType attributeType;
        
        public enum ModifyValueType { Add, Multiply, Override, Veto }
        public ModifyValueType modifyValueType;

        public enum ChangeType { Level, Min, Max }

        public MinMaxCurve defaultLevelCurveChange;
        public float defaultLevelChange;
        public float defaultMinChange;
        public float defaultMaxChange;

        [Serializable]
        public class PrefabVariantValueMapping
        {
            public int prefabVariantIndex;
            public MinMaxCurve levelCurveChange;
            public float levelChange;            
            public float minChange;
            public float maxChange;
        }
        public List<PrefabVariantValueMapping> prefabVariantValueMappings;

        // Returns PositiveInfinity if the curve is null
        public float GetValue(int prefabVariantIndex, ChangeType changeType, bool useExpectedValue = false)
        {
            if (modifierType == ModifierType.OnAction)
            {
                MinMaxCurve minMaxCurve = defaultLevelCurveChange;
                if (prefabVariantValueMappings != null && prefabVariantValueMappings.Count > 0)
                {
                    PrefabVariantValueMapping valueMapping = prefabVariantValueMappings.Find(x => x.prefabVariantIndex == prefabVariantIndex);
                    if (valueMapping != null)
                    {
                         minMaxCurve = valueMapping.levelCurveChange;
                    }
                }

                if (minMaxCurve == null)
                    return float.PositiveInfinity;
                return useExpectedValue ? minMaxCurve.ExpectedValue() : minMaxCurve.EvalRandom();
            }

            // Always Modifier Type - does not use curves
            float changeValue = 0f;
            if (prefabVariantValueMappings != null && prefabVariantValueMappings.Count > 0)
            {
                PrefabVariantValueMapping valueMapping = prefabVariantValueMappings.Find(x => x.prefabVariantIndex == prefabVariantIndex);
                if (valueMapping != null)
                {
                    switch (changeType)
                    {
                        case ChangeType.Level:
                            changeValue = valueMapping.levelChange;
                            break;
                        case ChangeType.Min:
                            changeValue = valueMapping.minChange;
                            break;
                        case ChangeType.Max:
                            changeValue = valueMapping.maxChange;
                            break;
                    }
                    return changeValue;
                }
            }
            switch (changeType)
            {
                case ChangeType.Level:
                    changeValue = defaultLevelChange;
                    break;
                case ChangeType.Min:
                    changeValue = defaultMinChange;
                    break;
                case ChangeType.Max:
                    changeValue = defaultMaxChange;
                    break;
            }
            return changeValue;
        }
    }
}
