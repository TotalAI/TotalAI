using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(DriveType))]
    public class DriveTypeEditor : InputOutputTypeEditor
    {
        private GUIStyle headerStyle;
        private DriveType driveType;

        private void OnEnable()
        {
            headerStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };

            driveType = (DriveType)target;

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sideEffectValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("typeCategories"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("showInUI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("priority"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("continueModifier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canCauseInterruptions"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("syncType"));

            if (serializedObject.FindProperty("syncType").enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("syncAttributeType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("syncAttributeDirection"));
            }

            if (serializedObject.FindProperty("syncType").enumValueIndex == 2)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("driveTypeEquation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeInSECalculations"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("utilityCurve"), GUILayout.Height(150));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("changeType"));

            if (serializedObject.FindProperty("changeType").enumValueIndex != 0)
            { 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rateTimeCurve"), GUILayout.Height(150));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minTimeCurve"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxTimeCurve"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("changePerGameHour"));
            }

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("rateAgeCurve"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}