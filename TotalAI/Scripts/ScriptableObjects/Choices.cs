using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Choices", menuName = "Total AI/Choices", order = 2)]
    public class Choices : ScriptableObject
    {
        [Header("Set either Choices Type OR Choices and Default Value")]
        public ChoicesType choicesType;
        public List<string> stringChoices;
        public List<float> floatChoices;
        public List<int> intChoices;
        public List<UnityEngine.Object> unityObjectChoices;
        public int defaultIndex;

        public enum TypeName { String, Float, Int, UnityObject }
        public TypeName typeName;

        public T FixedValue<T>(Agent agent, int index)
        {
            return GetChoices<T>(agent)[index];
        }

        public T DefaultValue<T>(Agent agent)
        {
            return GetChoices<T>(agent)[defaultIndex];
        }

        public int NumberChoices(Agent agent)
        {
            if (choicesType != null)
                return choicesType.NumberChoices(agent);

            switch (typeName)
            {
                case TypeName.String:
                    return stringChoices.Count;
                case TypeName.Float:
                    return floatChoices.Count;
                case TypeName.Int:
                    return intChoices.Count;
                case TypeName.UnityObject:
                    return unityObjectChoices.Count;
            }
            return 0;
        }

        public string[] OptionNames(Agent agent = null)
        {
            if (choicesType != null)
                return choicesType.OptionNames(agent);

            switch (typeName)
            {
                case TypeName.String:
                    return stringChoices.ToArray();
                case TypeName.Float:
                    return floatChoices.Select(x => x.ToString()).ToArray();
                case TypeName.Int:
                    return intChoices.Select(x => x.ToString()).ToArray();
                case TypeName.UnityObject:
                    return unityObjectChoices.Select(x => x.name).ToArray();
            }
            return null;
        }

        public List<T> GetChoices<T>(Agent agent = null)
        {
            if (choicesType != null)
                return choicesType.GetChoices<T>(agent);

            switch (typeName)
            {
                case TypeName.String:
                    return stringChoices.Cast<T>().ToList();
                case TypeName.Float:
                    return floatChoices.Cast<T>().ToList();
                case TypeName.Int:
                    return intChoices.Cast<T>().ToList();
                case TypeName.UnityObject:
                    return unityObjectChoices.Cast<T>().ToList();
            }
            return null;
        }

        public Type ForType(Agent agent = null)
        {
            if (choicesType != null)
                return choicesType.ForType(agent);

            switch (typeName)
            {
                case TypeName.String:
                    return typeof(string);
                case TypeName.Float:
                    return typeof(float);
                case TypeName.Int:
                    return typeof(int);
                case TypeName.UnityObject:
                    return typeof(UnityEngine.Object);
            }
            return null;
        }

        // Editor Methods
        public void DrawFixedValueField(SerializedProperty sp)
        {
            int selection = EditorGUILayout.IntPopup("Fixed Enum Value", (int)sp.FindPropertyRelative("fixedValue").floatValue, OptionNames(),
                                                     Enumerable.Range(0, NumberChoices(null)).ToArray());
            sp.FindPropertyRelative("fixedValue").floatValue = selection;
        }

        public void DrawFixedValueField(SerializedProperty sp, Rect rect)
        {
            int selection = EditorGUI.IntPopup(rect, "Fixed Enum Value", (int)sp.FindPropertyRelative("fixedValue").floatValue, OptionNames(),
                                                     Enumerable.Range(0, NumberChoices(null)).ToArray());
            sp.FindPropertyRelative("fixedValue").floatValue = selection;
        }
    }
}
