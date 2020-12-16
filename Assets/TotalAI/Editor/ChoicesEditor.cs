using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(Choices))]
    public class ChoicesEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("choicesType"));
            GUILayout.Space(20);

            ChoicesType choicesType = (ChoicesType)serializedObject.FindProperty("choicesType").objectReferenceValue;

            if (choicesType == null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("typeName"));
                int typeName = serializedObject.FindProperty("typeName").enumValueIndex;
                switch (typeName)
                {
                    case (int)Choices.TypeName.Float:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("floatChoices"));
                        break;
                    case (int)Choices.TypeName.String:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stringChoices"));
                        break;
                    case (int)Choices.TypeName.Int:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("intChoices"));
                        break;
                    case (int)Choices.TypeName.UnityObject:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("unityObjectChoices"));
                        break;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultIndex"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
