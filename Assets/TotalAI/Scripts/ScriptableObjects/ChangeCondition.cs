using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ChangeCondition", menuName = "Total AI/Change Condition", order = 1)]
    public class ChangeCondition : ScriptableObject
    {
        public enum ValueType
        {
            None, BoolValue, FloatValue, IntValue, StringValue, ActionSkillCurve, UnityObject, Selector
        }

        public ChangeConditionType changeConditionType;

        public EntityType entityType;
        public LevelType levelType;

        public ValueType valueType;
        public bool boolValue;
        public float floatValue;
        public int intValue;
        public string stringValue;
        public MinMaxCurve actionSkillCurve;
        public Object unityObject;
        public Selector selector;
    }
}
