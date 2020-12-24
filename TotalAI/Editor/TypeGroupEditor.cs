using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(TypeGroup))]
    public class TypeGroupEditor : UnityEditor.Editor
    {
        private GUIStyle headerStyle;
        private TypeGroup grouping;

        private List<EntityType> matches;

        private void OnEnable()
        {
            grouping = (TypeGroup)target;
        }

        public override void OnInspectorGUI()
        {
            headerStyle = new GUIStyle("label")
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            if (matches == null || EditorGUI.EndChangeCheck())
            {
                List<EntityType> allEntityTypes = new List<EntityType>();
                var guids = AssetDatabase.FindAssets("t:EntityType");
                foreach (string guid in guids)
                {
                    allEntityTypes.Add((EntityType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                }
                grouping.ResetCachedMatches();
                matches = grouping.GetMatches(allEntityTypes);
            }

            // Allow Updating all TypeCategories Children
            GUILayout.Space(25);
            if (GUILayout.Button("Update All TypeCategories Children", GUILayout.Width(300f)))
            {
                List<TypeCategory> allTypeCategories = new List<TypeCategory>();
                var guids = AssetDatabase.FindAssets("t:TypeCategory");
                foreach (string guid in guids)
                {
                    allTypeCategories.Add((TypeCategory)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
                }

                foreach (TypeCategory typeCategory in allTypeCategories)
                {
                    typeCategory.children = new List<TypeCategory>();
                }
                foreach (TypeCategory typeCategory in allTypeCategories)
                {
                    EditorUtility.SetDirty(typeCategory);
                    foreach (TypeCategory typeCategory2 in allTypeCategories)
                    {
                        if (typeCategory.parent == typeCategory2)
                        {
                            typeCategory2.children.Add(typeCategory);
                            break;
                        }
                    }
                }
                AssetDatabase.SaveAssets();
            }
            
            GUILayout.Space(25);
            GUILayout.Label("Grouping Matches:", headerStyle);
            GUILayout.Space(10);

            // List Matches
            for (int i = 0; i < matches.Count; i++)
            {
                GUILayout.Label((i + 1) + ": " + matches[i].name);
            }
            if (matches.Count == 0)
            {
                GUILayout.Label("No Matches");
            }
        }
    }
}
