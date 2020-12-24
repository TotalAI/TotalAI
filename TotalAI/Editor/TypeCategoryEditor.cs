using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(TypeCategory))]
    public class TypeCategoryEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;
        private TypeCategory selectedTypeCategory;

        private List<InputOutputType> matches;

        private void OnEnable()
        {
            selectedTypeCategory = (TypeCategory)target;
        }

        public override void OnInspectorGUI()
        {
            headerStyle = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            TypeCategory parent = (TypeCategory)EditorGUILayout.ObjectField("Parent", serializedObject.FindProperty("parent").objectReferenceValue,
                                                                            typeof(TypeCategory), false);

            if (parent != selectedTypeCategory)
                serializedObject.FindProperty("parent").objectReferenceValue = parent;
            else
                Debug.LogError("Blocked TypeCategory (" + selectedTypeCategory.name + ") parent from pointing at itself.");

            if (matches == null || EditorGUI.EndChangeCheck())
            {
                List<InputOutputType> allIOTs = new List<InputOutputType>();
                var guids = AssetDatabase.FindAssets("t:InputOutputType");
                foreach (string guid in guids)
                {
                    allIOTs.Add((InputOutputType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                }

                matches = selectedTypeCategory.IsCategoryOrDescendantOf(allIOTs);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("children"));
            EditorGUI.EndDisabledGroup();

            // Allow Updating all TypeCategories Children
            GUILayout.Space(20);
            if (GUILayout.Button("Update All Type Categories Children", GUILayout.Width(300f)))
            {
                TypeCategory.UpdateTypeCategoriesChildren();
                Repaint();
            }
            
            GUILayout.Space(20);
            GUILayout.Label("InputOutputTypes of " + selectedTypeCategory.name + " or a Descendant", headerStyle);
            GUILayout.Space(8);

            // List Matches
            for (int i = 0; i < matches.Count; i++)
            {
                GUILayout.Label((i + 1) + ": " + matches[i].name + " (" + matches[i].TypeCategoriesToString() + ")");
            }
            if (matches.Count == 0)
            {
                GUILayout.Label("No Matches");
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}
