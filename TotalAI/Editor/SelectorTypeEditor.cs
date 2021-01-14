using UnityEngine;
using UnityEditor;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(SelectorType))]
    public class SelectorTypeEditor : UnityEditor.Editor
    {
        private SelectorType selectorType;

        private void OnEnable()
        {
            selectorType = (SelectorType)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startType"));
            if ((SelectorType.StartType)serializedObject.FindProperty("startType").enumValueIndex == SelectorType.StartType.FixedValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("startFixedValue"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("minType"));
            if ((SelectorType.MinType)serializedObject.FindProperty("minType").enumValueIndex == SelectorType.MinType.FixedValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minFixedValue"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxType"));
            if ((SelectorType.MaxType)serializedObject.FindProperty("maxType").enumValueIndex == SelectorType.MaxType.FixedValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFixedValue"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("combineType"));

            SerializedProperty selectorFactorInfos = serializedObject.FindProperty("selectorFactorInfos");
            selectorFactorInfos.arraySize = EditorGUILayout.DelayedIntField("How Many Selector Factors?", selectorFactorInfos.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < selectorFactorInfos.arraySize; i++)
            {
                SerializedProperty selectorFactorInfo = selectorFactorInfos.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(selectorFactorInfo.FindPropertyRelative("selectorFactor"), new GUIContent((i + 1) + ". Selector Factor"));
                EditorGUILayout.PropertyField(selectorFactorInfo.FindPropertyRelative("weight"), new GUIContent((i + 1) + ". Weight"));
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}