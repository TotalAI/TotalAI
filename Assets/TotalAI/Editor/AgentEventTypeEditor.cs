using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(AgentEventType))]
    public class AgentEventTypeEditor : EntityTypeEditor
    {
        private float lineHeight;

        public static bool showTiming;
        public static bool showAttendees;
        public static bool showSound;
        public static bool showRoleTypes;

        private Texture2D attendeesIcon;
        private Texture2D rolesIcon;
        private Texture2D timingIcon;
        private Texture2D soundIcon;

        private AgentEventType agentEventType;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            attendeesIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/people.png", typeof(Texture2D)) as Texture2D;
            rolesIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/roles.png", typeof(Texture2D)) as Texture2D;
            timingIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/clock.png", typeof(Texture2D)) as Texture2D;
            soundIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/sound.png", typeof(Texture2D)) as Texture2D;

            agentEventType = (AgentEventType)target;

            lineHeight = EditorGUIUtility.singleLineHeight;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            Texture2D whiteTexture = Texture2D.whiteTexture;
            GUIStyle style = new GUIStyle("box") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0) };
            style.normal.background = whiteTexture;

            Color defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorUtilities.L1;
            GUILayout.BeginVertical(style, GUILayout.Height(Screen.height - 155f));
            GUI.backgroundColor = defaultColor;

            Color sectionColor = EditorUtilities.L2;

            base.OnInspectorGUI();

            string rightInfo = "Max Wait " + serializedObject.FindProperty("maxTimeToWait").floatValue + "s";
            showTiming = EditorUtilities.BeginInspectorSection("Timing", timingIcon, showTiming, sectionColor, sectionBackground, rightInfo);
            if (showTiming)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxTimeToWait"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToWaitAfterMin"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("includeGoToAgents"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("runningTimeLimit"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("endWhenCreatorQuits"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("endWhenLessThanMin"));
            }
            EditorUtilities.EndInspectorSection(showTiming);

            rightInfo = serializedObject.FindProperty("minAttendees").intValue + " - " + serializedObject.FindProperty("maxAttendees").intValue;
            rightInfo += " : R Min = " + serializedObject.FindProperty("rLevelMin").floatValue;
            if (serializedObject.FindProperty("onlyFactionMembers").boolValue)
                rightInfo += " : Only Faction";
            showAttendees = EditorUtilities.BeginInspectorSection("Attendees", attendeesIcon, showAttendees, sectionColor, sectionBackground, rightInfo);
            if (showAttendees)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("onlyFactionMembers"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rLevelMin"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minAttendees"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAttendees"));
            }
            EditorUtilities.EndInspectorSection(showAttendees);

            if (serializedObject.FindProperty("soundForWaiting").objectReferenceValue != null)
                rightInfo = serializedObject.FindProperty("soundForWaiting").objectReferenceValue.name;
            else
                rightInfo = "None";
            if (serializedObject.FindProperty("loop").boolValue)
                rightInfo += " (loop)";
            showSound = EditorUtilities.BeginInspectorSection("Sound", soundIcon, showSound, sectionColor, sectionBackground, rightInfo);
            if (showSound)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("soundForWaiting"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loop"));
            }
            EditorUtilities.EndInspectorSection(showSound);

            int numRoleMappings = serializedObject.FindProperty("attendeeRoleMappings").arraySize;
            rightInfo = numRoleMappings > 0 ? numRoleMappings.ToString() : "None";
            showRoleTypes = EditorUtilities.BeginInspectorSection("Role Types", rolesIcon, showRoleTypes, sectionColor, sectionBackground, rightInfo);
            if (showRoleTypes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("roleMappingType"));
                GUILayout.Space(15f);

                SerializedProperty attendeeRoleMappings = serializedObject.FindProperty("attendeeRoleMappings");
                attendeeRoleMappings.arraySize = EditorGUILayout.DelayedIntField("How Many Mappings?", attendeeRoleMappings.arraySize);
                GUILayout.Space(5f);

                AgentEventType.RoleMappingType roleMappingType =
                    (AgentEventType.RoleMappingType)serializedObject.FindProperty("roleMappingType").enumValueIndex;
                for (int i = 0; i < attendeeRoleMappings.arraySize; i++)
                {
                    GUILayout.Label("Mapping #" + (i + 1));
                    SerializedProperty attendeeRoleMapping = attendeeRoleMappings.GetArrayElementAtIndex(i);
                    EditorGUI.indentLevel++;
                    switch (roleMappingType)
                    {
                        case AgentEventType.RoleMappingType.CreatorAttendee:
                            EditorGUILayout.PropertyField(attendeeRoleMapping.FindPropertyRelative("forCreator"));
                            break;
                        case AgentEventType.RoleMappingType.JoinOrder:
                            EditorGUILayout.PropertyField(attendeeRoleMapping.FindPropertyRelative("mappingJoinOrders"));
                            break;
                        case AgentEventType.RoleMappingType.HasRoleType:
                            EditorGUILayout.PropertyField(attendeeRoleMapping.FindPropertyRelative("mappingRoleTypes"));
                            break;
                    }
                    EditorGUILayout.PropertyField(attendeeRoleMapping.FindPropertyRelative("roleTypes"));
                    EditorGUI.indentLevel--;
                    GUILayout.Space(5f);

                }

            }
            EditorUtilities.EndInspectorSection(showRoleTypes);

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
    }
}
 