using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(Agent))]
    public class AgentEditor : EntityEditor
    {
        private static bool showMainInfo;
        private static bool showRoleTypes;

        private ReorderableList roleTypesList;
        private SerializedProperty serializedRoleTypes;

        private Texture2D personIcon;
        private Texture2D rolesIcon;

        private Agent agent;

        public override void OnEnable()
        {
            agent = (Agent)target;

            base.OnEnable();

            personIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/person.png", typeof(Texture2D)) as Texture2D;
            rolesIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/roles.png", typeof(Texture2D)) as Texture2D;

            serializedRoleTypes = serializedObject.FindProperty("roleTypes");
            roleTypesList = new ReorderableList(serializedObject, serializedRoleTypes, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawRoleTypeItems,
                onAddCallback = OnAddRoleType,
                headerHeight = 1
            };
        }

        private void OnAddRoleType(ReorderableList list)
        {
            serializedRoleTypes.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawRoleTypeItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = roleTypesList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            string rightInfo;

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
                showMainInfo = true;

            rightInfo = entity.entityType?.name;
            if (agent.faction != null)
                rightInfo += " (" + agent.faction.name + ")";
            showMainInfo = EditorUtilities.BeginInspectorSection("Basic Info", personIcon, showMainInfo, sectionColor, sectionBackground, rightInfo);
            if (showMainInfo)
            {
                if (entity.entityType == null)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.HelpBox(ObjectNames.NicifyVariableName(type.Name) + " must be specified.", MessageType.Error);
                    GUILayout.Space(5);
                }
                EntityType entityType = (EntityType)EditorGUILayout.ObjectField("Agent Type", entity.entityType, typeof(AgentType), false);
                serializedObject.FindProperty("entityType").objectReferenceValue = entityType;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabVariantIndex"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gender"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("startAge"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("faction"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gLevel"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mainLoopInterval"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxMainLoopRandomWait"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("corpseWorldObjectType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("corpsePrefabVariantIndex"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("makeNewCorpseGameObject"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dieAnimatorBoolParamName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dieAnimatorStateName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dieWaitTime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveInventoryToCorpse"));
            }
            EditorUtilities.EndInspectorSection(showMainInfo);

            if (agent.roleTypes != null && agent.roleTypes.Count > 3)
            {
                rightInfo = agent.roleTypes[0].name + ", " + agent.roleTypes[1].name + ", " +
                            agent.roleTypes[2].name + " (" + agent.roleTypes.Count + ")";
            }
            else
            {
                rightInfo = agent.roleTypes == null || agent.roleTypes.Count == 0 ?
                            "None" : string.Join(", ", agent.roleTypes.Select(x => x == null ? "Null" : x.name));
            }

            showRoleTypes = EditorUtilities.BeginInspectorSection("Role Types", rolesIcon, showRoleTypes, sectionColor, sectionBackground, rightInfo);
            if (showRoleTypes)
            {
                roleTypesList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showRoleTypes);

            base.OnInspectorGUI();

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
    }
}
 