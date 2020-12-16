using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(InputConditionType), true)]
    public class InputConditionTypeEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;
        private InputConditionType inputConditionType;

        private void OnEnable()
        {
            inputConditionType = (InputConditionType)target;
        }

        public override void OnInspectorGUI()
        {
            headerStyle = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            headerStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);

            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;
            
            if ((inputConditionType.typeInfo.usesTypeGroup || inputConditionType.typeInfo.usesTypeGroupFromIndex) &&
                (inputConditionType.typeInfo.usesInventoryTypeGroup || inputConditionType.typeInfo.usesInventoryTypeGroupShareWith))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Can NOT have any OCT matches since it this ICT is for an Entity Target AND an Inventory Target.");
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("matchingOCTs"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
