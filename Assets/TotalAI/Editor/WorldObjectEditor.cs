using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(WorldObject))]
    public class WorldObjectEditor : EntityEditor
    {
        private string newWorldObjectTypeName;
        private string newWorldObjectTypeDir;
        private string newWorldObjectPrefabDir;

        private WorldObject worldObject;
        private TotalAIManager totalAIManager;
        private int selectedNewTypeIndex;
        //private readonly string[] newTypeOptions = new string[] { "Tool or Weapon", "Projectile Weapon", "Agent Goes In", "Uses Resources",
        //                                                          "Fixed Storage", "Carriable Storage", "No Inventory" };
        public override void OnEnable()
        {
            base.OnEnable();

            worldObject = (WorldObject)target;
            totalAIManager = FindObjectOfType<TotalAIManager>();
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

            //UnityEngine.Object entityType = serializedObject.FindProperty("entityType").objectReferenceValue;
            GUILayout.Space(15);
            Type type = EntityType.GetEntityType(entity);
            if (entity.entityType == null)
            {
                EditorGUILayout.HelpBox(ObjectNames.NicifyVariableName(type.Name) + " must be specified.", MessageType.Error);
                GUILayout.Space(5);
            }

            EditorGUI.indentLevel++;
            EntityType entityType = (EntityType)EditorGUILayout.ObjectField("World Object Type", entity.entityType, typeof(WorldObjectType), false);
            serializedObject.FindProperty("entityType").objectReferenceValue = entityType;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabVariantIndex"));
            EditorGUI.indentLevel--;
            GUILayout.Space(10);

            WorldObjectType worldObjectType = entity.entityType as WorldObjectType;
            if (entity.entityType != null && worldObject != null && worldObjectType.states != null && worldObjectType.states.Count > 0)
            {
                EditorGUI.indentLevel++;
                List<string> stateNames = worldObjectType.StateNames();
                string startStateName = serializedObject.FindProperty("startState").FindPropertyRelative("name").stringValue;
                int currentSelection = stateNames.IndexOf(startStateName);
                int selectedIndex = EditorGUILayout.Popup("Start State", currentSelection, stateNames.ToArray());
                if (currentSelection != selectedIndex)
                {
                    worldObject.startState = worldObjectType.states.Find(x => x.name == stateNames[selectedIndex]);
                    EditorUtility.SetDirty(worldObject);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("runOutputChangesOnStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("startCompletePoints"));
                EditorGUI.indentLevel--;
                GUILayout.Space(10);
            }

            if (Application.isPlaying && entity.gameObject.activeInHierarchy && entity.entityType != null)
            {
                if (worldObject.tags != null && worldObject.tags.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Tags", string.Join(", ", worldObject.tags.Select(x => x.Key.name)),
                                               new GUIStyle("boldLabel"));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                if (worldObject.inEntityInventory)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("In Inventory Of", worldObject.inEntityInventory.name,
                                               new GUIStyle("boldLabel"));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                if (worldObject.worldObjectType.states != null && worldObject.worldObjectType.states.Count > 0 && worldObject.currentState != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Current State", worldObject.currentState.name, new GUIStyle("boldLabel"));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                if (worldObject.worldObjectType.completeType != WorldObjectType.CompleteType.None)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Complete Points", worldObject.completePoints + " / " + worldObject.worldObjectType.pointsToComplete,
                                               new GUIStyle("boldLabel"));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                if (worldObject.worldObjectType.canBeDamaged)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Damage Points", worldObject.damage + " / " + worldObject.worldObjectType.damageToDestroy,
                                               new GUIStyle("boldLabel"));
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
            }

            if (entity.entityType == null)
            {
                GUILayout.BeginVertical("helpbox");

                bool allowCreate = true;
                EditorGUI.indentLevel++;
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Quick Create WorldObjectType", new GUIStyle("boldLabel"));
                GUILayout.Space(8);

                if (!Directory.Exists(newWorldObjectTypeDir))
                {
                    allowCreate = false;
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("World Object Type Directory does not exist.", MessageType.Error);
                    GUILayout.Space(5);
                }
                if (string.IsNullOrEmpty(newWorldObjectTypeDir) && totalAIManager != null && totalAIManager.settings != null)
                    newWorldObjectTypeDir = totalAIManager.settings.scriptableObjectsDirectory + "/WorldObjectTypes";
                newWorldObjectTypeDir = EditorGUILayout.TextField("World Object Type Directory", newWorldObjectTypeDir);

                if (!Directory.Exists(newWorldObjectPrefabDir))
                {
                    allowCreate = false;
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("World Object Prefab Directory does not exist.", MessageType.Error);
                    GUILayout.Space(5);
                }
                if (string.IsNullOrEmpty(newWorldObjectPrefabDir) && totalAIManager != null && totalAIManager.settings != null)
                    newWorldObjectPrefabDir = totalAIManager.settings.prefabsDirectory + "/WorldObjects";
                newWorldObjectPrefabDir = EditorGUILayout.TextField("World Object Prefab Directory", newWorldObjectPrefabDir);

                if (string.IsNullOrEmpty(newWorldObjectTypeName))
                    newWorldObjectTypeName = worldObject.name;
                string potentialPath = newWorldObjectTypeDir + "/" + newWorldObjectTypeName + ".asset";
                if (AssetDatabase.AssetPathToGUID(potentialPath) != null && AssetDatabase.AssetPathToGUID(potentialPath) != "" &&
                    AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(potentialPath) != null)
                {
                    allowCreate = false;
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("World Object Type of that name and that directory already exists: '" + potentialPath + "'", MessageType.Error);
                    GUILayout.Space(5);
                }

                if (Application.isPlaying)
                    allowCreate = false;

                newWorldObjectTypeName = EditorGUILayout.TextField("Name", newWorldObjectTypeName);

                //selectedNewTypeIndex = EditorGUILayout.Popup("World Object Type Template", selectedNewTypeIndex, newTypeOptions);

                GUILayout.BeginHorizontal(new GUIStyle("label") { margin = new RectOffset((int)EditorGUIUtility.labelWidth, 0, 10, 0) });
                EditorGUI.BeginDisabledGroup(!allowCreate);
                bool create = GUILayout.Button("Create New World Object Type", new GUIStyle("button") { padding = new RectOffset(8, 8, 8, 8) },
                                               GUILayout.Width(240f));
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                GUILayout.Space(10);
                GUILayout.EndVertical();

                // TODO: Move this into WorldObjecTypeTemplate SO
                if (create)
                {
                    // Makes sure layer is correct
                    if (worldObject.gameObject.layer != LayerMask.NameToLayer("WorldObject"))
                    {
                        SetLayerRecursively(worldObject, worldObject.gameObject, LayerMask.NameToLayer("WorldObject"), false);
                        Debug.Log("WorldObject's GameObject's layer and all children's layers set to 'WorldObject'.");
                    }

                    // Make sure has a collider
                    if (totalAIManager.settings.for2D)
                    {
                        Collider2D collider2D = worldObject.GetComponent<Collider2D>();
                        if (collider2D == null)
                        {
                            worldObject.gameObject.AddComponent<Collider2D>();
                            Debug.Log("Collider2D added to WorldObject's GameObject.");
                        }
                    }
                    else
                    {
                        Collider collider = worldObject.GetComponent<Collider>();
                        if (collider == null)
                        {
                            worldObject.gameObject.AddComponent<BoxCollider>();
                            Debug.Log("BoxCollider added to WorldObject's GameObject.");
                        }
                    }

                    if (!Directory.Exists(newWorldObjectTypeDir))
                    {
                        Debug.LogError("Directory '" + newWorldObjectTypeDir + "' does not exist.  Please fix.");
                        return;
                    }

                    WorldObjectType newWorldObjectType = CreateInstance<WorldObjectType>();

                    string fullPath = newWorldObjectTypeDir + "/" + newWorldObjectTypeName + ".asset";
                    fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);
                    AssetDatabase.CreateAsset(newWorldObjectType, fullPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    newWorldObjectType = AssetDatabase.LoadAssetAtPath<WorldObjectType>(fullPath);

                    serializedObject.FindProperty("entityType").objectReferenceValue = newWorldObjectType;
                    serializedObject.ApplyModifiedProperties();

                    GameObject prefab;
                    GameObject transformMappingGO;
                    GameObject transformMappingPrefab;
                    if (!PrefabUtility.IsPartOfAnyPrefab(worldObject.gameObject))
                    {
                        Debug.Log("World Object is NOT a prefab - Creating Prefab.");
                        if (!Directory.Exists(newWorldObjectPrefabDir))
                        {
                            Debug.LogError("Directory '" + newWorldObjectPrefabDir + "' does not exist.  Unable to create WorldObject Prefab.");
                            return;
                        }
                        string rootPath = newWorldObjectPrefabDir;
                        string guid = AssetDatabase.CreateFolder(rootPath, newWorldObjectTypeName);
                        string newPath = AssetDatabase.GUIDToAssetPath(guid) + "/" + newWorldObjectTypeName + ".prefab";

                        // Make sure the file name is unique, in case an existing Prefab has the same name.
                        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

                        // Create the new Prefab.
                        prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(worldObject.gameObject, newPath, InteractionMode.UserAction);
                        prefab.transform.position = Vector3.zero;
                        Debug.Log("Prefab created at: " + newPath);

                        // Create an empty GO and make it a prefab for TransformMappings
                        string transformMappingPrefabName = newWorldObjectTypeName + "TransformMapping";
                        transformMappingGO = new GameObject(transformMappingPrefabName);
                        newPath = AssetDatabase.GUIDToAssetPath(guid) + "/" + transformMappingPrefabName + ".prefab";
                        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                        transformMappingPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(transformMappingGO, newPath, InteractionMode.UserAction);
                        transformMappingPrefab.transform.position = Vector3.zero;
                        DestroyImmediate(transformMappingGO);
                        Debug.Log("TransformMapping Prefab created at: " + newPath);
                    }
                    else
                    {
                        Debug.Log("World Object is already a prefab.");
                        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(worldObject.gameObject);
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    }

                    // Grab prefab and set it as the first prefabVariant
                    newWorldObjectType.prefabVariants = new List<GameObject>
                    {
                        prefab
                    };

                    // Set defaultInventory
                    newWorldObjectType.defaultInventoryType = totalAIManager.settings.defaultInventoryType;
                    EditorUtility.SetDirty(newWorldObjectType);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            base.OnInspectorGUI();

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        //TODO: Create utility class and move this there
        public static void SetLayerRecursively(Entity entityToChange, GameObject gameObject, int newLayer, bool checkForInventory)
        {
            // If we find an entity that is in inventory stop that branch
            if (checkForInventory)
            {
                Entity entity = gameObject.GetComponent<Entity>();
                if (entity != null && entity != entityToChange)
                {
                    // Entity inside entity so its inventory - quit this branch
                    return;
                }
            }

            gameObject.layer = newLayer;

            if (gameObject.transform.childCount != 0)
            {
                foreach (Transform child in gameObject.transform)
                {
                    SetLayerRecursively(entityToChange, child.gameObject, newLayer, checkForInventory);
                }
            }
        }
    }
}
 