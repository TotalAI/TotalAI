using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(TotalAIManager))]
    public class TotalAIManagerEditor : UnityEditor.Editor
    {
        private TotalAIManager manager;

        private void OnEnable()
        {
            manager = (TotalAIManager)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"));

            GUILayout.Space(10f);
            if (GUILayout.Button("Refresh Lists - Done Automatically On Play", GUILayout.Width(400)))
            {
                TotalAIManager.CreateTypeLists(manager);
            }
            GUILayout.Space(10f);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allTypeCategories"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allInputOutputTypes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allEntityTypes"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allTypeGroups"));
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
