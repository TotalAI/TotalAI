using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(BehaviorType), true)]
    public class BehaviorTypeEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;
        private BehaviorType behaviorType;

        private List<EntityType> matches;

        private void OnEnable()
        {
            behaviorType = (BehaviorType)target;
        }

        public override void OnInspectorGUI()
        {
            headerStyle = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            headerStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            Type[] requiredAttributeTypes = behaviorType.editorHelp.requiredAttributeTypes;
            string[] requiredDescriptions = behaviorType.editorHelp.requiredDescriptions;

            if (requiredAttributeTypes != null && requiredAttributeTypes.Length != 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("AttributeTypes: These will be modified while the BehaviorType is running.", headerStyle);
                for (int i = 0; i < requiredAttributeTypes.Length; i++)
                {
                    GUILayout.Label((i + 1) + ". "+ requiredAttributeTypes[i].Name + ": " + requiredDescriptions[i]);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredAttributeTypes"));
            }

            Type[] types = behaviorType.editorHelp.valueTypes;
            string[] descriptions = behaviorType.editorHelp.valueDescriptions;

            if (types.Length != 0)
            {
                if (types.Length != serializedObject.FindProperty("defaultSelectors").arraySize)
                {
                    serializedObject.FindProperty("defaultSelectors").arraySize = types.Length;
                }
                SerializedProperty defaultSelectors = serializedObject.FindProperty("defaultSelectors");

                //EditorGUILayout.PropertyField(defaultSelectors);
                for (int i = 0; i < types.Length; i++)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Default Selector: " + descriptions[i], headerStyle);
                    GUILayout.Space(5);
                    DrawSelector(serializedObject.FindProperty("defaultSelectors").GetArrayElementAtIndex(i), types[i]);
                }
            }

            if (behaviorType is GoToBT)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDistanceTolerance"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateTowardsTarget"));
                if (serializedObject.FindProperty("rotateTowardsTarget").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRotationDegreesDelta"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationTolerance"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("reduceEnergy"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("changeSpeedDuringBehavior"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("recalculateDestination"));

            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("afterStartWaitTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("afterUpdateWaitTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beforeFinishWaitTime"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSelector(SerializedProperty sp, Type type)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative("valueType"));
            int valueType = sp.FindPropertyRelative("valueType").enumValueIndex;
            switch (valueType)
            {
                case (int)Selector.ValueType.AttributeType:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("attributeType"), type);
                    break;
                case (int)Selector.ValueType.MinMax:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("minMax"), typeof(MinMax));
                    break;
                case (int)Selector.ValueType.Choices:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("choices"), typeof(Choices));
                    break;
            }

            // TODO:  If its Choices with a ChoiceType (dynamic) - Fixed can't be selected since we don't know the choices
            // Need to remove option from selectionType

            EditorGUILayout.PropertyField(sp.FindPropertyRelative("selectionType"));
            switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
            {
                case (int)Selector.SelectionType.FixedValue:
                    // Show the field based on the attributeType
                    if (valueType == (int)Selector.ValueType.AttributeType)
                    {
                        AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                        if (attributeType != null)
                        {
                            attributeType.DrawFixedValueField(sp);
                        }
                    }
                    else if (valueType == (int)Selector.ValueType.MinMax)
                    {
                        MinMax minMax = (MinMax)sp.FindPropertyRelative("minMax").objectReferenceValue;
                        if (minMax != null)
                        {
                            EditorGUILayout.PropertyField(sp.FindPropertyRelative("fixedValue"));
                        }
                    }
                    else
                    {
                        Choices choices = (Choices)sp.FindPropertyRelative("choices").objectReferenceValue;
                        if (choices != null)
                        {
                            choices.DrawFixedValueField(sp);
                        }
                    }
                    break;
                case (int)Selector.SelectionType.SelectorType:

                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("selectorType"));
                    break;
            }
        }
    }
}
