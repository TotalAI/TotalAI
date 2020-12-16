using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(ActionType))]
    public class ActionTypeEditor : InputOutputTypeEditor
    {
        private GUIStyle headerStyle;
        private ActionType actionType;

        private void OnEnable()
        {

            actionType = (ActionType)target;

        }
        public override void OnInspectorGUI()
        {
            headerStyle = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sideEffectValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("typeCategories"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("entityTargetSearchRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistanceAsInput"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("interruptType"));

            if (serializedObject.FindProperty("interruptType").enumValueIndex != (int)ActionType.InterruptType.Never)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("replanFrequency"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minGreaterUtilityToInterrupt"));
                if (serializedObject.FindProperty("interruptType").enumValueIndex == (int)ActionType.InterruptType.OnlySpecifiedDrives)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interruptingDriveTypes"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("noWaitOnInterrupt"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noWaitOnFinishNoNextMapping"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noWaitOnFinishHasNextMapping"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("usesInventorySlots"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mapping Types", headerStyle);

            if (actionType.mappingTypes == null || actionType.mappingTypes.Count == 0)
            {
                GUILayout.Label("None - Please add this Action Type to a Mapping Type.");
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUIUtility.labelWidth = 15.0f;
                    for (int i = 0; i < serializedObject.FindProperty("mappingTypes").arraySize; i++)
                    {
                        GUIContent content = new GUIContent((i + 1) + ".");
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("mappingTypes").GetArrayElementAtIndex(i), content);
                    }
                    EditorGUIUtility.labelWidth = 0f;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Refresh Mapping Types", GUILayout.Width(200)))
            {
                List<MappingType> allMappingTypes = new List<MappingType>();
                var guids = AssetDatabase.FindAssets("t:MappingType");
                foreach (string guid in guids)
                {
                    allMappingTypes.Add((MappingType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                }

                actionType.mappingTypes = new List<MappingType>();
                foreach (MappingType mappingType in allMappingTypes)
                {
                    if (mappingType.actionType == actionType)
                        actionType.mappingTypes.Add(mappingType);
                }

                EditorUtility.SetDirty(actionType);
                AssetDatabase.SaveAssets();
            }
        }
    }
}