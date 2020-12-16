using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(ChangeCondition))]
    public class ChangeConditionEditor : UnityEditor.Editor
    {
        private ChangeCondition changeCondition;

        private void OnEnable()
        {
            changeCondition = (ChangeCondition)target;
        }

        public override void OnInspectorGUI()
        {
            GUIStyle infoBoxStyle = EditorStyles.helpBox;
            //infoBoxStyle.border = new RectOffset(10, 10, 10, 10);
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixels(new Color[] { new Color(0.3f, 0.3f, 0.3f) });
            texture.Apply();
            infoBoxStyle.normal.background = texture;
            infoBoxStyle.richText = true;
            infoBoxStyle.wordWrap = true;
            infoBoxStyle.fontSize = 12;
            infoBoxStyle.padding = new RectOffset(20, 20, 10, 10);
            infoBoxStyle.alignment = TextAnchor.MiddleLeft;

            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            ChangeConditionType changeConditionType = (ChangeConditionType)serializedObject.FindProperty("changeConditionType").objectReferenceValue;
            if (changeConditionType != null && changeConditionType.typeInfo.description != null && changeConditionType.typeInfo.description.Length > 0)
            {
                GUILayout.Space(10f);
                EditorGUILayout.LabelField(changeConditionType.typeInfo.description, infoBoxStyle);
                GUILayout.Space(10f);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("changeConditionType"));
            if (EditorGUI.EndChangeCheck())
            {
                changeConditionType = (ChangeConditionType)serializedObject.FindProperty("changeConditionType").objectReferenceValue;
                if (changeConditionType != null)
                {
                    // Move enums to first valid options
                    serializedObject.FindProperty("valueType").enumValueIndex = (int)changeConditionType.typeInfo.possibleValueTypes[0];
                }
            }
            
            if (changeConditionType != null)
            {
                ChangeConditionType.TypeInfo typeInfo = changeConditionType.typeInfo;

                ChangeCondition.ValueType valueType;
                using (new EditorGUI.DisabledScope(typeInfo.possibleValueTypes.Length == 1))
                {
                    valueType = (ChangeCondition.ValueType)serializedObject.FindProperty("valueType").enumValueIndex;
                    valueType = (ChangeCondition.ValueType)EditorGUILayout.EnumPopup(new GUIContent("Value Type To Use"), valueType,
                                                                                     changeConditionType.CheckEnabledValueType, false);
                    serializedObject.FindProperty("valueType").enumValueIndex = (int)valueType;
                }

                switch (valueType)
                {
                    case ChangeCondition.ValueType.BoolValue:
                        GUIContent boolLabel = new GUIContent(typeInfo.boolLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("boolValue"), boolLabel);
                        break;
                    case ChangeCondition.ValueType.FloatValue:
                        GUIContent floatLabel = new GUIContent(typeInfo.floatLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), floatLabel);
                        break;
                    case ChangeCondition.ValueType.IntValue:
                        GUIContent intLabel = new GUIContent(typeInfo.intLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("intValue"), intLabel);
                        break;
                    case ChangeCondition.ValueType.StringValue:
                        GUIContent stringLabel = new GUIContent(typeInfo.stringLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"), stringLabel);
                        break;
                    case ChangeCondition.ValueType.ActionSkillCurve:
                        GUIContent actionSkillCurveLabel = new GUIContent(typeInfo.actionSkillCurveLabel);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionSkillCurve"), actionSkillCurveLabel);
                        break;
                    case ChangeCondition.ValueType.UnityObject:
                        GUIContent unityObjectLabel = new GUIContent(typeInfo.unityObjectLabel);
                        UnityEngine.Object unityObject = serializedObject.FindProperty("unityObject").objectReferenceValue;
                        unityObject = EditorGUILayout.ObjectField(unityObjectLabel, unityObject, typeInfo.unityObjectType, false);
                        serializedObject.FindProperty("unityObject").objectReferenceValue = unityObject;
                        break;
                    case ChangeCondition.ValueType.Selector:
                        SerializedProperty sp = serializedObject.FindProperty("selector");
                        EditorGUILayout.PropertyField(sp.FindPropertyRelative("valueType"));

                        switch (sp.FindPropertyRelative("valueType").enumValueIndex)
                        {
                            case (int)Selector.ValueType.AttributeType:
                                EditorGUILayout.ObjectField(sp.FindPropertyRelative("attributeType"), typeof(AttributeType));
                                break;
                            case (int)Selector.ValueType.MinMax:
                                EditorGUILayout.ObjectField(sp.FindPropertyRelative("minMax"), typeof(MinMax));
                                break;
                            case (int)Selector.ValueType.Choices:
                                EditorGUILayout.ObjectField(sp.FindPropertyRelative("choices"), typeof(Choices));
                                break;
                        }

                        EditorGUILayout.PropertyField(sp.FindPropertyRelative("selectionType"));

                        switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
                        {
                            case (int)Selector.SelectionType.FixedValue:
                                int selectorValueType = sp.FindPropertyRelative("valueType").enumValueIndex;

                                // Show the field based on the attributeType
                                AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                if (selectorValueType == (int)Selector.ValueType.AttributeType)
                                {
                                    AttributeType selectorAttributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                    if (selectorAttributeType != null)
                                    {
                                        selectorAttributeType.DrawFixedValueField(sp);
                                    }
                                }
                                else if (selectorValueType == (int)Selector.ValueType.MinMax)
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
                        break;
                }

                if (typeInfo.usesEntityType)
                {
                    UnityEngine.Object entityType = serializedObject.FindProperty("entityType").objectReferenceValue;
                    GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(typeInfo.entityTypeLabel));
                    entityType = EditorGUILayout.ObjectField(label, entityType, typeInfo.mostRestrictiveEntityType, false);
                    serializedObject.FindProperty("entityType").objectReferenceValue = entityType;
                }

                if (typeInfo.usesLevelType)
                {
                    UnityEngine.Object levelType = serializedObject.FindProperty("levelType").objectReferenceValue;
                    GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(typeInfo.levelTypeLabel));
                    levelType = EditorGUILayout.ObjectField(label, levelType, typeInfo.mostRestrictiveLevelType, false);
                    serializedObject.FindProperty("levelType").objectReferenceValue = levelType;
                }

                if (typeInfo.usesBoolValue)
                {
                    GUIContent boolLabel = new GUIContent(typeInfo.boolLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("boolValue"), boolLabel);
                }
                if (typeInfo.usesFloatValue)
                {
                    GUIContent floatLabel = new GUIContent(typeInfo.floatLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), floatLabel);
                }
                if (typeInfo.usesIntValue)
                {
                    GUIContent intLabel = new GUIContent(typeInfo.intLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), intLabel);
                }
                if (typeInfo.usesStringValue)
                {
                    GUIContent stringLabel = new GUIContent(typeInfo.stringLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"), stringLabel);
                }
                if (typeInfo.usesSelector)
                {
                    EditorGUILayout.LabelField(typeInfo.selectorLabel);
                    EditorGUI.indentLevel++;
                    SerializedProperty sp = serializedObject.FindProperty("selector");
                    EditorGUILayout.PropertyField(sp.FindPropertyRelative("valueType"));

                    switch (sp.FindPropertyRelative("valueType").enumValueIndex)
                    {
                        case (int)Selector.ValueType.AttributeType:
                            EditorGUILayout.ObjectField(sp.FindPropertyRelative("attributeType"), typeof(AttributeType));
                            break;
                        case (int)Selector.ValueType.MinMax:
                            EditorGUILayout.ObjectField(sp.FindPropertyRelative("minMax"), typeof(MinMax));
                            break;
                        case (int)Selector.ValueType.Choices:
                            EditorGUILayout.ObjectField(sp.FindPropertyRelative("choices"), typeof(Choices));
                            break;
                    }

                    EditorGUILayout.PropertyField(sp.FindPropertyRelative("selectionType"));

                    switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
                    {
                        case (int)Selector.SelectionType.FixedValue:
                            int selectorValueType = sp.FindPropertyRelative("valueType").enumValueIndex;

                            // Show the field based on the attributeType
                            AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                            if (selectorValueType == (int)Selector.ValueType.AttributeType)
                            {
                                AttributeType selectorAttributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                if (selectorAttributeType != null)
                                {
                                    selectorAttributeType.DrawFixedValueField(sp);
                                }
                            }
                            else if (selectorValueType == (int)Selector.ValueType.MinMax)
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

                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}