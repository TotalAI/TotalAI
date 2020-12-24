using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "UtilityModifier", menuName = "Total AI/Utility Modifier", order = 1)]
    public class UtilityModifier : ScriptableObject
    {
        public UtilityModifierType utilityModifierType;

        public EntityType entityType;
        public LevelType levelType;
        public bool boolValue;
        public float floatValue;
        public int intValue;
        public string stringValue;
        public MinMaxCurve minMaxCurve;
        public Object unityObject;
        public Selector selector;
    }
}
