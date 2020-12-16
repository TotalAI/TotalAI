using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TotalAI.Editor
{

    [CustomEditor(typeof(EntityModifier))]
    public class EntityModifierEditor : UnityEditor.Editor
    {
        private float lineHeight;

        private GUIStyle headerStyle;
        private GUIStyle infoBoxStyle;
        
        private ReorderableList inputConditionList;
        private ReorderableList outputChangeList;

        private SerializedProperty serializedInputConditions;
        private SerializedProperty serializedOutputChanges;

        private void OnEnable()
        {
            lineHeight = EditorGUIUtility.singleLineHeight;
            headerStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            headerStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            serializedInputConditions = serializedObject.FindProperty("inputConditions");
            inputConditionList = new ReorderableList(serializedObject, serializedInputConditions, true, true, true, true)
            {
                drawElementCallback = DrawInputConditionListItems,
                drawHeaderCallback = DrawInputConditionHeader,
                onAddCallback = OnAddInputCondition,
                elementHeightCallback = InputConditionHeight
            };

            serializedOutputChanges = serializedObject.FindProperty("outputChanges");
            outputChangeList = new ReorderableList(serializedObject, serializedOutputChanges, true, true, true, true)
            {
                drawElementCallback = DrawOutputChangeListItems,
                drawHeaderCallback = DrawOutputChangeHeader,
                onAddCallback = OnAddOutputChange,
                elementHeightCallback = OutputChangeHeight
            };
        }

        private void DrawInputConditionHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Input Conditions", headerStyle);
        }

        private void OnAddInputCondition(ReorderableList list)
        {
            serializedInputConditions.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            EditorUtilities.OnAddInputCondition(element);
        }

        private float InputConditionHeight(int index)
        {
            SerializedProperty element = inputConditionList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorUtilities.InputConditionHeight(null, element, index, infoBoxStyle);
        }

        private void DrawInputConditionListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = inputConditionList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawInputConditionItem(null, element, index, rect, infoBoxStyle);
        }

        private void DrawOutputChangeHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, 200f, lineHeight + 5f), "Output Changes", headerStyle);
        }

        private void OnAddOutputChange(ReorderableList list)
        {
            serializedOutputChanges.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            EditorUtilities.OnAddOutputChange(element);
        }

        private float OutputChangeHeight(int index)
        {
            SerializedProperty element = outputChangeList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorUtilities.OutputChangeHeight(index, null, element, infoBoxStyle, false, true);
        }

        private void DrawOutputChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = outputChangeList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawOutputChangeItem(index, null, element, rect, infoBoxStyle, false, true);
        }

        public override void OnInspectorGUI()
        {
            infoBoxStyle = EditorStyles.helpBox;
            //infoBoxStyle.border = new RectOffset(10, 10, 10, 10);
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixels(new Color[] { new Color(0.3f, 0.3f, 0.3f) });
            texture.Apply();
            infoBoxStyle.normal.background = texture;
            infoBoxStyle.richText = true;
            infoBoxStyle.wordWrap = true;
            infoBoxStyle.fontSize = 12;
            infoBoxStyle.padding = new RectOffset(20, 20, 5, 5);
            infoBoxStyle.alignment = TextAnchor.MiddleLeft;

            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("forTarget"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelType"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMatchType"));
            
            InputCondition.MatchType ownerMatchType = (InputCondition.MatchType)serializedObject.FindProperty("targetMatchType").enumValueIndex;
            switch (ownerMatchType)
            {
                case InputCondition.MatchType.TypeGroup:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("typeGroup"));
                    break;
                case InputCondition.MatchType.TypeCategory:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("typeCategory"));
                    break;
                case InputCondition.MatchType.EntityType:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("entityType"));
                    break;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("coolDown"));

            GUILayout.Space(10f);
            inputConditionList.DoLayoutList();

            GUILayout.Space(10f);
            outputChangeList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}