using System;
using System.Collections.Generic;

namespace TotalAI
{
    [Serializable]
    public class ItemActionSkillModifier
    {
        public ItemCondition itemCondition;

        public List<ActionType> actionTypes;

        public enum ModifyValueType { Add, Multiply, Override, Veto }
        public ModifyValueType modifyValueType;

        public MinMaxCurve defaultValueCurve;

        [Serializable]
        public class PrefabVariantValueMapping
        {
            public int prefabVariantIndex;
            public MinMaxCurve valueCurve;
        }
        public List<PrefabVariantValueMapping> prefabVariantValueMappings;

        public bool Check(Agent agent, InventoryType.Item item, ActionType actionType)
        {
            if (!actionTypes.Contains(actionType))
                return false;

            return itemCondition.Check(agent, actionType, null, item.inventorySlot, 0, false, true, true);
        }

        public float GetValue(int prefabVariantIndex, bool useExpectedValue = false)
        {
            if (prefabVariantValueMappings == null || prefabVariantValueMappings.Count == 0)
                return useExpectedValue ? defaultValueCurve.ExpectedValue() : defaultValueCurve.EvalRandom();

            PrefabVariantValueMapping valueMapping = prefabVariantValueMappings.Find(x => x.prefabVariantIndex == prefabVariantIndex);
            if (valueMapping == null)
                return useExpectedValue ? defaultValueCurve.ExpectedValue() : defaultValueCurve.EvalRandom();

            return useExpectedValue ? valueMapping.valueCurve.ExpectedValue() : valueMapping.valueCurve.EvalRandom();
        }
    }
}
