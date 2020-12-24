using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(MinMax))]
    public class MinMaxEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("minMaxType"));
            GUILayout.Space(20);

            MinMaxType minMaxType = (MinMaxType)serializedObject.FindProperty("minMaxType").objectReferenceValue;

            if (minMaxType == null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("min"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("max"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultValue"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
