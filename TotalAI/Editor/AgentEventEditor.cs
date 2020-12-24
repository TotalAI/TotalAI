using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(AgentEvent))]
    public class AgentEventEditor : EntityEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Texture2D whiteTexture = Texture2D.whiteTexture;
            GUIStyle style = new GUIStyle("box") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0) };
            style.normal.background = whiteTexture;

            Color defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorUtilities.L15;
            GUILayout.BeginVertical(style);
            GUI.backgroundColor = defaultColor;

            Color sectionColor = EditorUtilities.L2;

            Type type = EntityType.GetEntityType(entity);
            if (entity.entityType == null)
            {
                GUILayout.Space(15);
                EditorGUILayout.HelpBox(ObjectNames.NicifyVariableName(type.Name) + " must be specified.", MessageType.Error);
            }

            GUILayout.Space(15);
            EditorGUI.indentLevel++;
            EntityType entityType = (EntityType)EditorGUILayout.ObjectField("Agent Event Type", entity.entityType, typeof(AgentEventType), false);
            serializedObject.FindProperty("entityType").objectReferenceValue = entityType;
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("state"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("creator"));

            if (!serializedObject.FindProperty("attendees").isExpanded)
                serializedObject.FindProperty("attendees").isExpanded = true;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attendees"));
            
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
    }
}
 