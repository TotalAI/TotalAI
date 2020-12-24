using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(RoleType))]
    public class RoleTypeEditor : InputOutputTypeEditor
    {
        private GUIStyle headerStyle;
        private GUIStyle infoBoxStyle;
        private float lineHeight;

        private RoleType roleType;

        private ReorderableList inputConditionList;
        private ReorderableList driveChangesList;
        private ReorderableList actionChangesList;

        private SerializedProperty serializedInputConditions;
        private SerializedProperty serializedDriveChanges;
        private SerializedProperty serializedActionChanges;

        private void OnEnable()
        {
            roleType = (RoleType)target;

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
            serializedDriveChanges = serializedObject.FindProperty("driveChanges");
            driveChangesList = new ReorderableList(serializedObject, serializedDriveChanges, true, true, true, true)
            {
                drawElementCallback = DrawDriveChangeListItems,
                drawHeaderCallback = DrawDriveChangesHeader,
                onAddCallback = OnAddDriveChange,
                elementHeightCallback = DriveChangesHeight
            };
            serializedActionChanges = serializedObject.FindProperty("actionChanges");
            actionChangesList = new ReorderableList(serializedObject, serializedActionChanges, true, true, true, true)
            {
                drawElementCallback = DrawActionChangeListItems,
                drawHeaderCallback = DrawActionChangesHeader,
                onAddCallback = OnAddActionChange,
                elementHeightCallback = ActionChangesHeight
            };
        }

        private void DrawActionChangesHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Action Changes", headerStyle);
        }

        private void OnAddActionChange(ReorderableList list)
        {
            serializedActionChanges.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("changeType").enumValueIndex = 0;
            element.FindPropertyRelative("actionType").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
            element.FindPropertyRelative("changeProbability").floatValue = 0;
            element.FindPropertyRelative("changeAmount").floatValue = 0;
            element.FindPropertyRelative("priority").intValue = 0;
        }

        private float ActionChangesHeight(int index)
        {
            SerializedProperty element = actionChangesList.serializedProperty.GetArrayElementAtIndex(index);
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 6f;

            return height + 20f;
        }

        private void DrawActionChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = actionChangesList.serializedProperty.GetArrayElementAtIndex(index);
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changeType"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("actionType"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("level"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changeProbability"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changeAmount"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("priority"));
        }

        private void DrawDriveChangesHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Drive Changes", headerStyle);
        }

        private void OnAddDriveChange(ReorderableList list)
        {
            serializedDriveChanges.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("changeType").enumValueIndex = 0;
            element.FindPropertyRelative("driveType").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
            element.FindPropertyRelative("changePerGameHour").floatValue = 0;
            element.FindPropertyRelative("minTimeCurve").floatValue = 0;
            element.FindPropertyRelative("maxTimeCurve").floatValue = 0;
            element.FindPropertyRelative("priority").intValue = 0;
        }

        private float DriveChangesHeight(int index)
        {
            SerializedProperty element = driveChangesList.serializedProperty.GetArrayElementAtIndex(index);
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 2f;

            DriveType driveType = (DriveType)element.FindPropertyRelative("driveType").objectReferenceValue;
            if (driveType != null)
            {
                height += itemHeight * 3f;
                if (driveType.changeType == DriveType.RateOfChangeType.TimeOfDayCurve)
                    height += itemHeight * 3f;
            }
            return height + 20f;
        }

        private void DrawDriveChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = driveChangesList.serializedProperty.GetArrayElementAtIndex(index);
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changeType"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("driveType"));

            DriveType driveType = (DriveType)element.FindPropertyRelative("driveType").objectReferenceValue;
            if (driveType != null)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("priority"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("level"));

                if (driveType.changeType == DriveType.RateOfChangeType.Constant)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changePerGameHour"));
                }
                else
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight + itemHeight), element.FindPropertyRelative("rateTimeCurve"));
                    currentY += itemHeight * 2;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("minTimeCurve"));
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("maxTimeCurve"));
                }
            }
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

        public override void OnInspectorGUI()
        {
            infoBoxStyle = EditorStyles.helpBox;
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixels(new Color[] { new Color(0.3f, 0.3f, 0.3f) });
            texture.Apply();
            infoBoxStyle.normal.background = texture;
            infoBoxStyle.richText = true;
            infoBoxStyle.wordWrap = true;
            infoBoxStyle.fontSize = 12;
            infoBoxStyle.padding = new RectOffset(20, 20, 5, 5);
            infoBoxStyle.alignment = TextAnchor.MiddleLeft;

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("sideEffectValue"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("typeCategories"));

            GUILayout.Space(10f);
            inputConditionList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideType"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("overridePlannerType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDefaultMappingType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("overrideDefaultDriveType"));

            GUILayout.Space(20f);
            driveChangesList.DoLayoutList();

            GUILayout.Space(10f);
            actionChangesList.DoLayoutList();


            serializedObject.ApplyModifiedProperties();
        }
    }
}