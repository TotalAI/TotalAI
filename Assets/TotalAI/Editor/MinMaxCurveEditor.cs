using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(MinMaxCurve)), CanEditMultipleObjects]
    public class MinMaxCurveEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("min"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("max"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"), GUILayout.Height(200));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
