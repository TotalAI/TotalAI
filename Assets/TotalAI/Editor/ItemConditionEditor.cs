using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(ItemCondition))]
    public class ItemConditionEditor : UnityEditor.Editor
    {
        private ItemCondition itemCondition;

        private void OnEnable()
        {
            itemCondition = (ItemCondition)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("modifierType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isTarget"));

            SerializedProperty actionTypes = serializedObject.FindProperty("actionTypes");
            actionTypes.arraySize = EditorGUILayout.DelayedIntField("How Many Action Types?", actionTypes.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < actionTypes.arraySize; i++)
            {
                SerializedProperty actionType = actionTypes.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(actionType, new GUIContent((i + 1) + ". Action Type"));
            }
            EditorGUI.indentLevel--;

            SerializedProperty otherActionTypes = serializedObject.FindProperty("otherActionTypes");
            otherActionTypes.arraySize = EditorGUILayout.DelayedIntField("How Many Other Action Types?", otherActionTypes.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < otherActionTypes.arraySize; i++)
            {
                SerializedProperty otherActionType = otherActionTypes.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(otherActionType, new GUIContent((i + 1) + ". Other Action Type"));
            }
            EditorGUI.indentLevel--;

            SerializedProperty inventorySlots = serializedObject.FindProperty("inventorySlots");
            inventorySlots.arraySize = EditorGUILayout.DelayedIntField("How Many Inventory Slots?", inventorySlots.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < inventorySlots.arraySize; i++)
            {
                SerializedProperty inventorySlot = inventorySlots.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(inventorySlot, new GUIContent((i + 1) + ". Inventory Slot"));
            }
            EditorGUI.indentLevel--;

            SerializedProperty agentTypeConstraints = serializedObject.FindProperty("agentTypeConstraints");
            agentTypeConstraints.arraySize = EditorGUILayout.DelayedIntField("How Many Agent Type Constraints?", agentTypeConstraints.arraySize);
            EditorGUI.indentLevel++;
            for (int i = 0; i < agentTypeConstraints.arraySize; i++)
            {
                SerializedProperty agentTypeConstraint = agentTypeConstraints.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(agentTypeConstraint.FindPropertyRelative("agentType"),
                                                new GUIContent(i + 1 + ". Agent Type"));

                SerializedProperty prefabVariantsIndexes = agentTypeConstraint.FindPropertyRelative("prefabVariantsIndexes");
                EditorGUI.indentLevel++;
                prefabVariantsIndexes.arraySize = EditorGUILayout.DelayedIntField("How Many Indexes?", prefabVariantsIndexes.arraySize);
                for (int j = 0; j < prefabVariantsIndexes.arraySize; j++)
                {
                    SerializedProperty prefabVariantsIndex = prefabVariantsIndexes.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(prefabVariantsIndex, new GUIContent(j + 1 + ". Prefab Variant Index"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }
}