using System;
using System.Collections.Generic;

namespace TotalAI
{
    [Serializable]
    public class Selector
    {
        // Values
        public enum ValueType { AttributeType, MinMax, Choices }
        public ValueType valueType;
        public AttributeType attributeType;
        public MinMax minMax;
        public Choices choices;
        
        // Selection
        public enum SelectionType { CurrentOrDefaultValue, FixedValue, SelectorType }
        public SelectionType selectionType;
        public float fixedValue;
        public SelectorType selectorType;

        public T GetEnumValue<T>(Agent agent, Mapping mapping)
        {
            switch (selectionType)
            {
                case SelectionType.CurrentOrDefaultValue:
                    return CurrentEnumValue<T>(agent);
                case SelectionType.FixedValue:
                    return FixedEnumValue<T>(agent);
                case SelectionType.SelectorType:
                    return SelectorTypeEnumValue<T>(agent, mapping);
            }

            return default;
        }

        private T CurrentEnumValue<T>(Agent agent)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    return agent.attributes[attributeType].GetEnumValue<T>();
                case ValueType.Choices:
                    return choices.DefaultValue<T>(agent);
            }

            return default;
        }

        private T FixedEnumValue<T>(Agent agent)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    return ((EnumAT)attributeType).OptionValue<T>(agent, (int)fixedValue);
                case ValueType.Choices:
                    return choices.FixedValue<T>(agent, (int)fixedValue);
            }

            return default;
        }

        private T SelectorTypeEnumValue<T>(Agent agent, Mapping mapping)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    EnumAT enumAT = (EnumAT)attributeType;
                    int selectedIndex = selectorType.EvaluateEnumSelectorFactors(this, agent, mapping, enumAT.NumberOptions());
                    return enumAT.OptionValue<T>(agent, selectedIndex);
                case ValueType.Choices:
                    int choicesIndex = selectorType.EvaluateEnumSelectorFactors(this, agent, mapping, choices.NumberChoices(agent));
                    return choices.FixedValue<T>(agent, choicesIndex);
            }

            return default;
        }

        public float GetFloatValue(Agent agent, Mapping mapping)
        {
            switch (selectionType)
            {
                case SelectionType.CurrentOrDefaultValue:
                    return CurrentFloatValue(agent);
                case SelectionType.FixedValue:
                    return fixedValue;
                case SelectionType.SelectorType:
                    return selectorType.EvaluateFloatSelectorFactors(this, agent, mapping, MinFloatValue(agent), MaxFloatValue(agent),
                                                                     CurrentFloatValue(agent));
            }

            return 0;
        }

        private float MinFloatValue(Agent agent)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    return agent.attributes[attributeType].GetMin();
                case ValueType.MinMax:
                    return minMax.GetMin(agent);
            }
            return 0;
        }

        private float MaxFloatValue(Agent agent)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    return agent.attributes[attributeType].GetMax();
                case ValueType.MinMax:
                    return minMax.GetMax(agent);
            }
            return 0;
        }

        private float CurrentFloatValue(Agent agent)
        {
            switch (valueType)
            {
                case ValueType.AttributeType:
                    return agent.AttributeLevel(attributeType);
                case ValueType.MinMax:
                    return minMax.DefaultValue(agent);
            }
            return 0;
        }

        public bool IsValid()
        {
            if (selectionType == SelectionType.SelectorType && selectorType == null)
                return false;

            switch (valueType)
            {
                case ValueType.AttributeType:
                    return attributeType != null;
                case ValueType.MinMax:
                    return minMax != null;
                case ValueType.Choices:
                    return choices != null;
            }
            return false;
        }

        public Type SelectorForType()
        {
            if (!IsValid())
                return null;

            switch (valueType)
            {
                case ValueType.AttributeType:
                    return attributeType.ForType();
                case ValueType.MinMax:
                    return typeof(float);
                case ValueType.Choices:
                    return choices.ForType();
            }
            return null;
        }

        public override string ToString()
        {
            return ":" + selectionType + ":" + (selectorType != null ? selectorType.name : "");
        }

    }
}