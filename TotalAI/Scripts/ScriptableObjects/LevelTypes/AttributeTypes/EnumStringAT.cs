using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "EnumStringAT", menuName = "Total AI/Level Types/Attribute Types/Enum String", order = 0)]
    public class EnumStringAT : EnumAT
    {
        [Header("Possible values for this enum AttributeType")]
        public string[] options;

        public override T OptionValue<T>(Entity entity)
        {
            return (T)(object)options[entitiesData[entity].enumIndexValue];
        }

        public override T OptionValue<T>(Entity entity, int index)
        {
            return (T)(object)options[index];
        }

        public override int NumberOptions()
        {
            return options.Length;
        }

        public override string[] OptionNames()
        {
            return options;
        }

        public override void DrawFixedValueField(SerializedProperty sp)
        {
            int selection = EditorGUILayout.IntPopup("Fixed Enum Value", (int)sp.FindPropertyRelative("fixedValue").floatValue, OptionNames(),
                                                     Enumerable.Range(0, options.Length).ToArray());
            sp.FindPropertyRelative("fixedValue").floatValue = selection;
        }

        public override void DrawFixedValueField(SerializedProperty sp, Rect rect)
        {
            int selection = EditorGUI.IntPopup(rect, "Fixed Enum Value", (int)sp.FindPropertyRelative("fixedValue").floatValue, OptionNames(),
                                               Enumerable.Range(0, options.Length).ToArray());
            sp.FindPropertyRelative("fixedValue").floatValue = selection;
        }

        public override void DrawDefaultValueFields(Rect rect, SerializedProperty sp)
        {
            int enumIndexValue = sp.FindPropertyRelative("enumData.enumIndexValue").intValue;

            enumIndexValue = sp.FindPropertyRelative("enumData.enumIndexValue").intValue = EditorGUI.IntPopup(
                new Rect(rect.x + 235, rect.y, 150, EditorGUIUtility.singleLineHeight),
                enumIndexValue, OptionNames(), Enumerable.Range(0, options.Length).ToArray());

            sp.FindPropertyRelative("enumData.enumIndexValue").intValue = enumIndexValue;
        }

        public override Type ForType()
        {
            return typeof(string);
        }
    }
}