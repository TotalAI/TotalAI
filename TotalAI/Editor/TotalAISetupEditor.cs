using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TotalAI.GOAP;
using System.IO;
using System;

namespace TotalAI.Editor
{
    public class TotalAISetupEditor : EditorWindow
    {
        private GUIStyle guiStyle;
        private GUIStyle guiStyleSuccess;
        private GUIStyle guiStyleFailure;
        private GUIStyle guiStyleBox;
        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle toolBarStyle;
        private int currentTab;

        private bool createTotalAIManager;
        private bool createTotalAISettings;
        private bool createTimeManager;
        private bool createGOAPManager;
        private bool createLayers;
        private bool createFolders;
        private bool createPrefabsFolders;
        private bool createScriptableObjects;

        private string defaultSettingsPath = "Assets/ScriptableObjects/Settings/";
        private string defaultSettingsName = "TotalAISettings";
        private string defaultSOFolderRoot = "Assets/ScriptableObjects/";
        private string defaultPrefabsFolderRoot = "Assets/Prefabs/";
        private bool for2D;

        //private TotalAISettings settings;
        private TotalAIManager totalAIManager;
        
        private Vector2 matchingScrollPosition;
        private Vector2 settingsScrollPosition;
        private Vector2 setupScrollPosition;

        private Texture logoImage;

        private readonly string[] allTypeNameDirectories =
        {
            "ActionTypes", "AgentEventTypes", "AgentTypeOverrides", "AgentTypes", "AnimationTypes", "AttributeTypes", "BehaviorTypes",
            "ChangeConditions", "ChangeConditionTypes", "Choices", "ChoiceTypes", "DeciderTypes", "DriveEquationTypes", "DriveTypes",
            "EntityModifiers", "Factions", "HistoryTypes", "InputConditionTypes", "InteractionSpotTypes", "InventorySlotConditions",
            "InventorySlots", "InventoryTypes", "ItemConditions", "MappingTypes", "MemoryTypes", "MinMaxCurves", "MinMaxes",
            "MovementTypes", "OutputChangeTypes", "PlannerTypes", "RoleTypes", "SelectorFactors", "SelectorTypes",
            "SensorTypes", "TagTypes", "TargetFactors", "TypeCategories", "TypeGroups", "UtilityFunctionTypes",
            "UtilityModifiers", "UtilityModifierTypes", "WorldObjectTypes", "WOTInventoryRecipes"
        };

        // Directory -> SO Type Name
        private readonly Dictionary<string, string[]> coreSOTypesToCreate = new Dictionary<string, string[]>
        {
            { "OutputChangeTypes", new string[] { "DriveLevelOCT" } },
            { "InputConditionTypes", new string[] { "DriveLevelICT", "NearEntityICT", "CurrentActionTypeICT" } },
            { "BehaviorTypes", new string[] { "GoToBT", "NothingBT" } },
            { "AnimationTypes", new string[] { "AnimatorAT", "NoneAT" } },
            { "HistoryTypes", new string[] { "BaseHT", "NoneHT" } },
            { "PlannerTypes", new string[] { "GOAPPT", "FiniteStateMachinePT", "UtilityAIPT" } },
            { "DeciderTypes", new string[] { "BaseDT", "UtilityAIDT" } },
            { "MemoryTypes", new string[] { "BaseMemoryType" } },
            { "UtilityFunctionTypes", new string[] { "DriveUtilityUFT", "UtilityAIUFT" } },
            { "InventoryTypes", new string[] { "Base3DIT", "Base2DIT" } },
            { "InteractionSpotTypes", new string[] { "NearestInteractionDistanceIST", "ClosestSpotIST" } },
            { "MovementTypes", new string[] { "UnityNavMesh3DMT", "AStarPathfinding2DMT" } },
            { "InventorySlots", new string[] { "InventorySlot", "InventorySlot", "InventorySlot" } },
            { "AttributeTypes", new string[] { "MinMaxFloatAT", "MinMaxFloatAT" } },
            { "TypeCategories", new string[] { "TypeCategory" } },
            { "SensorTypes", new string[] { "Sphere3DST", "Circle2DST" } },
        };

        private readonly string[] inventorySlotNames = { "LeftHand", "RightHand", "InvisibleUnlimited" };
        private readonly string[] attributeTypeNames = { "MovementSpeedAT", "DetectionRadiusAT" };

        [MenuItem("Tools/Total AI/Setup Window", false, 0)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            TotalAISetupEditor window = (TotalAISetupEditor)GetWindow(typeof(TotalAISetupEditor), false, "TAI Setup");
            window.Show();
        }

        private void OnEnable()
        {
            // TODO: Better way to find this is customer moves folders?
            logoImage = (Texture)EditorGUIUtility.Load("Assets/TotalAI/Editor/Images/TotalAILogo.png");

            currentTab = 0;
            createTotalAIManager = true;
            createTotalAISettings = true;
            createTimeManager = true;
            createGOAPManager = true;
            createLayers = true;
            createFolders = true;

            totalAIManager = FindObjectOfType<TotalAIManager>();
        }

        private void OnGUI()
        {
            guiStyle = new GUIStyle("label")
            {
                richText = true,
                fontSize = 12,
                wordWrap = true
            };

            guiStyleSuccess = new GUIStyle(guiStyle)
            {
                normal = new GUIStyleState() { textColor = new Color(.1f, .8f, .1f) }
            };

            guiStyleFailure = new GUIStyle(guiStyle)
            {
                normal = new GUIStyleState() { textColor = Color.red }
            };

            headerStyle = new GUIStyle("label")
            {
                richText = true,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            subHeaderStyle = new GUIStyle(headerStyle)
            {
                fontSize = 14
            };

            guiStyleBox = EditorStyles.helpBox;
            guiStyleBox.margin = new RectOffset(20, 20, 0, 20);
            guiStyleBox.padding = new RectOffset(20, 20, 20, 20);
            guiStyleBox.border = new RectOffset(1, 1, 0, 1);

            toolBarStyle = new GUIStyle("button");
            toolBarStyle.margin = new RectOffset(20, 20, 0, 0);

            GUILayout.Space(10);
            currentTab = GUILayout.Toolbar(currentTab, new string[] { "Setup", "Settings", "Matching" }, toolBarStyle);
            
            switch (currentTab)
            {
                case 0:
                    DrawSetup();
                    break;
                case 1:
                    DrawSettings();
                    break;
                case 2:
                    DrawMatching();
                    break;
            }
        }

        private void DrawSetup()
        {
            bool managerExists = false;
            bool settingsExist = false;
            bool coreSOsExist = false;
            TotalAIManager[] managers = FindObjectsOfType<TotalAIManager>();
            if (managers != null && managers.Length > 0)
            {
                managerExists = true;

                if (managers[0].settings != null)
                {
                    settingsExist = true;
                    string path = AssetDatabase.GetAssetPath(managers[0].settings);
                    defaultSettingsPath = Path.GetDirectoryName(path);
                    defaultSettingsName = Path.GetFileNameWithoutExtension(path);
                    defaultSOFolderRoot = managers[0].settings.scriptableObjectsDirectory;
                    defaultPrefabsFolderRoot = managers[0].settings.prefabsDirectory;

                    if (managers[0].settings.driveLevelICT != null)
                        coreSOsExist = true;
                }
            }

            bool timeManagerExists = false;
            TimeManager[] timeManagers = FindObjectsOfType<TimeManager>();
            if (timeManagers != null && timeManagers.Length > 0)
                timeManagerExists = true;

            bool goapManagerExists = false;
            GOAPPlannerManager[] goapManagers = FindObjectsOfType<GOAPPlannerManager>();
            if (goapManagers != null && goapManagers.Length > 0)
                goapManagerExists = true;

            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 300;

            EditorGUILayout.BeginVertical(guiStyleBox);

            DrawLogo();
            
            GUILayout.Label("Setup: Managers, Layers, and Folders", headerStyle);
            
            GUILayout.Space(8);

            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x, rect.y + 5), new Vector2(rect.x + rect.width, rect.y + 5));
            EditorGUILayout.EndHorizontal();

            setupScrollPosition = EditorGUILayout.BeginScrollView(setupScrollPosition);

            // Draw a bunch of checkboxes for things to setup and then one "Create Selected Items" button
            GUILayout.Space(20);
            EditorGUILayout.LabelField("1.  Create Managers: TotalAIManager, TimeManager, and GOAPPlannerManager", subHeaderStyle);
            GUILayout.Space(3);
            if (managerExists)
            {
                createTotalAIManager = false;
                createTotalAISettings = false;
                EditorGUILayout.LabelField("TotalAIManager Exists in the Scene.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(managerExists);
            createTotalAIManager = EditorGUILayout.ToggleLeft("Create TotalAIManager", createTotalAIManager);
            GUILayout.Space(3);
            EditorGUI.indentLevel++;
            createTotalAISettings = EditorGUILayout.ToggleLeft("Create TotalAISettings attached to TotalAIManager", createTotalAISettings);
            GUILayout.Space(2);
            EditorGUI.indentLevel += 2;
            defaultSettingsPath = EditorGUILayout.TextField("Directory For Settings ScriptableObject", defaultSettingsPath, GUILayout.Width(700));
            GUILayout.Space(2);
            defaultSettingsName = EditorGUILayout.TextField("Name For Settings ScriptableObject", defaultSettingsName, GUILayout.Width(500));
            GUILayout.Space(2);
            for2D = EditorGUILayout.Toggle("Is for 2D?", for2D, GUILayout.Width(500));

            EditorGUI.indentLevel -= 2;
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(3);
            if (timeManagerExists)
            {
                createTimeManager = false;
                EditorGUILayout.LabelField("TimeManager Exists in the Scene.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(timeManagerExists);
            createTimeManager = EditorGUILayout.ToggleLeft("Create TimeManager", createTimeManager);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(3);
            if (goapManagerExists)
            {
                createGOAPManager = false;
                EditorGUILayout.LabelField("GOAPPlannerManager Exists in the Scene.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(goapManagerExists);
            createGOAPManager = EditorGUILayout.ToggleLeft("Create GOAPPlannerManager", createGOAPManager);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(15);
            EditorGUILayout.LabelField("2.  Create Layers: Ground, Agent, WorldObject, AgentEvent, and Inventory", subHeaderStyle);
            GUILayout.Space(3);
            
            if (AllLayersExist())
            {
                EditorGUILayout.LabelField("All required layers exist.", guiStyleSuccess);
                createLayers = false;
            }
            else
            {
                createLayers = EditorGUILayout.ToggleLeft("Create Layers", createLayers);
            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField("3.  Create ScriptableObject Folders", subHeaderStyle);
            GUILayout.Space(3);
            if (Directory.Exists(defaultSOFolderRoot))
            {
                createFolders = false;
                EditorGUILayout.LabelField("ScriptableObject folder currently exists", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(Directory.Exists(defaultSOFolderRoot));
            createFolders = EditorGUILayout.ToggleLeft("Create ScriptableObjects Folders", createFolders);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(2);
            EditorGUI.indentLevel++;
            if (settingsExist)
            {
                EditorGUILayout.LabelField("TAI Settings exists - please set this in the settings.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(settingsExist);
            defaultSOFolderRoot = EditorGUILayout.TextField("Root Directory For ScriptableObjects", defaultSOFolderRoot, GUILayout.Width(700));
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            GUILayout.Space(15);
            
            EditorGUILayout.LabelField("4.  Create Prefabs Folders: Agents, AgentEvents, WorldObjects", subHeaderStyle);
            GUILayout.Space(3);
            if (Directory.Exists(defaultPrefabsFolderRoot))
            {
                createPrefabsFolders = false;
                EditorGUILayout.LabelField("Prefabs folder currently exists", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(Directory.Exists(defaultPrefabsFolderRoot));
            createPrefabsFolders = EditorGUILayout.ToggleLeft("Create Prefabs Folders", createPrefabsFolders);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(2);
            EditorGUI.indentLevel++;
            if (settingsExist)
            {
                EditorGUILayout.LabelField("TAI Settings exists - please set this in the settings.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(settingsExist);
            defaultPrefabsFolderRoot = EditorGUILayout.TextField("Root Directory For Prefabs", defaultPrefabsFolderRoot, GUILayout.Width(700));
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            GUILayout.Space(15);

            EditorGUILayout.LabelField("5.  Create Core ScriptableObjects", subHeaderStyle);
            GUILayout.Space(3);
            if (coreSOsExist)
            {
                createScriptableObjects = false;
                EditorGUILayout.LabelField("Core default ScriptableObjects exist.", guiStyleSuccess);
            }
            EditorGUI.BeginDisabledGroup(coreSOsExist);
            createScriptableObjects = EditorGUILayout.ToggleLeft("Create Core ScriptableObjects", createScriptableObjects);
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(20);
            
            bool noCreates = !createTotalAIManager && !createTimeManager && !createGOAPManager &&
                             !createFolders && !createLayers && !createScriptableObjects;
            if (noCreates)
            {
                EditorGUILayout.LabelField("Everything is setup.  Good Job!", new GUIStyle(guiStyleSuccess) { fontSize = 14 });
            }
            else
            {
                if (GUILayout.Button("Create", GUILayout.Width(200)))
                {
                    DoSetup();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalValue;
            EditorGUILayout.EndScrollView();
        }

        private void DrawLogo()
        {
            Rect logoRect = EditorGUILayout.BeginHorizontal();
            GUI.DrawTexture(new Rect(logoRect.x + logoRect.width - 340 / 3.7f, logoRect.y - 14, 339 / 3.7f, 141 / 3.7f), logoImage, ScaleMode.ScaleAndCrop, true);
            EditorGUILayout.EndHorizontal();
        }

        private bool AllLayersExist()
        {
            if (LayerMask.NameToLayer("Ground") == -1)
                return false;
            if (LayerMask.NameToLayer("Agent") == -1)
                return false;
            if (LayerMask.NameToLayer("WorldObject") == -1)
                return false;
            if (LayerMask.NameToLayer("AgentEvent") == -1)
                return false;
            if (LayerMask.NameToLayer("Inventory") == -1)
                return false;
            return true;
        }

        private void DoSetup()
        {
            Debug.Log("Starting Total AI Setup:");


            if (createFolders)
            {
                if (!defaultSOFolderRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    defaultSOFolderRoot += Path.DirectorySeparatorChar;
                }
                Debug.Log("Creating SO Folders");
                if (!Directory.Exists(defaultSOFolderRoot))
                {
                    Debug.Log("Creating Root Directory: " + defaultSOFolderRoot);
                    Directory.CreateDirectory(defaultSOFolderRoot);

                    Debug.Log("Creating Sub Directories");
                    foreach (string typeName in allTypeNameDirectories)
                    {
                        Directory.CreateDirectory(defaultSOFolderRoot + typeName);
                    }
                }
                else
                {
                    Debug.Log("Root Directory: " + defaultSOFolderRoot + " Already Exists.  NOT creating any folders.");
                }

                AssetDatabase.Refresh();
            }

            if (createPrefabsFolders)
            {
                if (!defaultPrefabsFolderRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    defaultPrefabsFolderRoot += Path.DirectorySeparatorChar;
                }
                Debug.Log("Creating Prefabs Folders");
                if (!Directory.Exists(defaultPrefabsFolderRoot))
                {
                    Debug.Log("Creating Root Directory: " + defaultPrefabsFolderRoot);
                    Directory.CreateDirectory(defaultPrefabsFolderRoot);

                    Debug.Log("Creating Sub Directories");
                    Directory.CreateDirectory(defaultPrefabsFolderRoot + "Agents");
                    Directory.CreateDirectory(defaultPrefabsFolderRoot + "WorldObjects");
                    Directory.CreateDirectory(defaultPrefabsFolderRoot + "AgentEvents");
                }
                else
                {
                    Debug.Log("Root Directory: " + defaultPrefabsFolderRoot + " Already Exists.  NOT creating any folders.");
                }

                AssetDatabase.Refresh();
            }


            if (createTotalAIManager)
            {
                GameObject manager = null;
                TotalAIManager totalAIManager = FindObjectOfType<TotalAIManager>();
                if (totalAIManager != null)
                {
                    Debug.LogError("TotalAIManager already exists in the Scene.  NOT creating a new one.");
                    manager = totalAIManager.gameObject;
                }
                else
                {
                    manager = new GameObject("TotalAIManager", typeof(TotalAIManager));
                    Debug.Log("Created TotalAIManager in the Scene.");
                }

                if (createTotalAISettings)
                {
                    if (!defaultSettingsPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                        !defaultSettingsPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    {
                        defaultSettingsPath += Path.DirectorySeparatorChar;
                    }
                    if (!Directory.Exists(defaultSettingsPath))
                    {
                        Debug.Log("Creating Directory: " + defaultSettingsPath);
                        Directory.CreateDirectory(defaultSettingsPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.Log("Directory Exists: " + defaultSettingsPath);
                    }

                    if (File.Exists(defaultSettingsPath + defaultSettingsName + ".asset"))
                    {
                        Debug.Log("Total AI Settings file already exists.  NOT overwritting it.");
                    }
                    else
                    {
                        TotalAISettings settings = CreateInstance<TotalAISettings>();
                        AssetDatabase.CreateAsset(settings, defaultSettingsPath + defaultSettingsName + ".asset");
                        manager.GetComponent<TotalAIManager>().settings = settings;

                        // TODO: Add in defaults - maybe search for commone ICTs and OCTs
                        settings.scriptableObjectsDirectory = defaultSOFolderRoot.Remove(defaultSOFolderRoot.Length - 1, 1);
                        settings.prefabsDirectory = defaultPrefabsFolderRoot.Remove(defaultPrefabsFolderRoot.Length - 1, 1);
                        settings.movementTypes = new List<MovementType>();
                        settings.for2D = for2D;

                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log("Created '" + defaultSettingsPath + defaultSettingsName + ".asset' and attached it to the TotalAIManager.");
                    }
                }
            }

            if (createTimeManager)
            {
                TimeManager timeManager = FindObjectOfType<TimeManager>();
                if (timeManager != null)
                {
                    Debug.Log("TimeManager already exists in the Scene.  NOT creating a new one.");
                }
                else
                {
                    new GameObject("TimeManager", typeof(TimeManager));
                    Debug.Log("Created TimeManager in the Scene.");
                }
            }

            if (createGOAPManager)
            {
                GOAPPlannerManager manager = FindObjectOfType<GOAPPlannerManager>();
                if (manager != null)
                {
                    Debug.Log("GOAPPlannerManager already exists in the Scene.  NOT creating a new one.");
                }
                else
                {
                    new GameObject("GOAPPlannerManager", typeof(GOAPPlannerManager));
                    Debug.Log("Created GOAPPlannerManager in the Scene.");
                }
            }

            if (createLayers)
            {
                Debug.Log("Creating layers: Ground, Agent, WorldObject, AgentEvent, Inventory");
                List<string> layersToCreate = new List<string>() { "Ground", "Agent", "WorldObject", "AgentEvent", "Inventory" };
                CreateLayers(layersToCreate);
            }

            if (createScriptableObjects)
            {
                CreateCoreScriptableObjects();
            }
        }

        private void CreateCoreScriptableObjects()
        {
            TotalAIManager totalAIManager = FindObjectOfType<TotalAIManager>();
            if (totalAIManager == null)
            {
                Debug.LogError("Trying to create core ScriptableObjects but there is no TotalAIManager.");
                return;
            }
            TotalAISettings settings = totalAIManager.settings;

            if (!Directory.Exists(defaultSOFolderRoot))
            {
                Debug.LogError("Trying to create core ScriptableObjects but root directory is missing: " + defaultSOFolderRoot);
                return;
            }
            if (!defaultSOFolderRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                defaultSOFolderRoot += Path.DirectorySeparatorChar;
            }

            InputConditionType currentActionTypeICT = null;
            AttributeType movementSpeedAT = null;
            AttributeType detectionRadiusAT = null;
            Sphere3DST sphere3DST = null;
            Circle2DST circle2DST = null;
            UtilityAIPT utilityAIPT = null;
            GoToBT goToBT = null;
            foreach (KeyValuePair<string, string[]> DirToSOType in coreSOTypesToCreate)
            {
                string directory = DirToSOType.Key;
                string[] typeNames = DirToSOType.Value;
                int inventorySlotNum = 0;
                int attributeTypeNum = 0;
                foreach (string typeName in typeNames)
                {
                    if (directory == "InventoryTypes" || directory == "MovementTypes" || directory == "SensorTypes")
                    {
                        if (typeName.Contains("2D") && !settings.for2D)
                            continue;
                        else if (typeName.Contains("3D") && settings.for2D)
                            continue;
                    }

                    string fullDirectoryPath = defaultSOFolderRoot + directory;
                    if (!Directory.Exists(fullDirectoryPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(fullDirectoryPath);
                            AssetDatabase.Refresh();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Trying to create directory for Core SO " + typeName + " at " + fullDirectoryPath + " - Error = " + e);
                            return;
                        }
                    }

                    string assetName = typeName;
                    if (typeName == "InventorySlot")
                    {
                        assetName = inventorySlotNames[inventorySlotNum];
                        ++inventorySlotNum;
                    }
                    else if (typeName == "MinMaxFloatAT")
                    {
                        assetName = attributeTypeNames[attributeTypeNum];
                        ++attributeTypeNum;
                    }
                    else if (typeName == "TypeCategory")
                    {
                        assetName = "All";
                    }

                    string fullPath = fullDirectoryPath + Path.DirectorySeparatorChar + assetName + ".asset";

                    // See if this Asset already exists
                    if (AssetDatabase.AssetPathToGUID(fullPath) != null && AssetDatabase.AssetPathToGUID(fullPath) != "" &&
                        AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath) != null)
                    {
                        Debug.Log("This core SO already exists: " + fullPath + " - Skipping it.");
                        continue;
                    }

                    ScriptableObject asset = CreateInstance(typeName);

                    if (asset == null)
                    {
                        Debug.LogError("Unable to create SO " + typeName + " - Skipping it.");
                        continue;
                    }

                    AssetDatabase.CreateAsset(asset, fullPath);

                    // See if this Asset has any Setup required
                    if (asset is InputConditionType inputConditionType)
                    {
                        if (inputConditionType.name == "DriveLevelICT")
                        {
                            if (settings.driveLevelOCT == null)
                            {
                                Debug.LogError("Trying to create DriveLevelICT and set its mathingOCTs to DriveLevelOCT but settings.driveLevelOCT " +
                                               "is not set.  Please set it and manually fix DriveLevelICT MatchingOCTs.");
                            }
                            else
                            {
                                inputConditionType.matchingOCTs = new List<OutputChangeType>
                                {
                                    settings.driveLevelOCT
                                };

                                EditorUtility.SetDirty(inputConditionType);
                            }

                            settings.driveLevelICT = inputConditionType;
                        }
                        else if (inputConditionType.name == "CurrentActionTypeICT")
                        {
                            currentActionTypeICT = inputConditionType;
                        }
                    }
                    else if (asset is OutputChangeType outputChangeType && outputChangeType.name == "DriveLevelOCT")
                    {
                        settings.driveLevelOCT = outputChangeType;
                    }
                    else if (asset is GOAPPT goapPT)
                    {
                        // Defaults to use the GOAPPT - should be attached to the GOAPPlannerManager
                        GOAPPlannerManager[] goapManagers = FindObjectsOfType<GOAPPlannerManager>();
                        if (goapManagers != null && goapManagers.Length > 0)
                        {
                            goapManagers[0].plannerTypeForGOAP = goapPT;
                        }
                        else
                        {
                            Debug.Log("Creating GOAPPT - No GOAPPlannerManager.");
                        }
                    }
                    else if (asset is UtilityAIPT)
                    {
                        utilityAIPT = (UtilityAIPT)asset;
                    }
                    else if (asset is FiniteStateMachinePT finiteStateMachinePT)
                    {
                        if (currentActionTypeICT != null)
                        {
                            finiteStateMachinePT.currentActionTypeICT = currentActionTypeICT;
                            EditorUtility.SetDirty(finiteStateMachinePT);
                        }
                    }
                    else if (asset is GoToBT)
                    {
                        goToBT = (GoToBT)asset;
                    }
                    else if (asset is BaseDT baseDT)
                    {
                        // Defaults to use the GOAPPT - should be attached to the GOAPPlannerManager
                        GOAPPlannerManager[] goapManagers = FindObjectsOfType<GOAPPlannerManager>();
                        if (goapManagers != null && goapManagers.Length > 0 && goapManagers[0].plannerTypeForGOAP != null)
                        {
                            baseDT.name = "GOAPDT";
                            baseDT.plannerTypes = new List<PlannerType>
                            {
                                goapManagers[0].plannerTypeForGOAP
                            };
                            EditorUtility.SetDirty(baseDT);
                        }
                        else
                        {
                            Debug.Log("Creating GOAPDT - unable to find the GOAPPT attached to the GOAPPlannerManager.");
                        }
                    }
                    else if (asset is UtilityAIDT utilityAIDT)
                    {
                        utilityAIDT.plannerTypes = new List<PlannerType>
                        {
                            utilityAIPT
                        };
                        EditorUtility.SetDirty(utilityAIDT);
                    }
                    else if (asset is InventoryType inventoryType)
                    {
                        settings.defaultInventoryType = inventoryType;
                    }
                    else if (asset is MovementType movementType)
                    {
                        settings.movementTypes.Add(movementType);
                    }
                    else if (asset is Sphere3DST)
                    {
                        sphere3DST = (Sphere3DST)asset;
                        if (detectionRadiusAT != null)
                        {
                            sphere3DST.radiusAttributeType = detectionRadiusAT;
                            EditorUtility.SetDirty(sphere3DST);
                        }
                    }
                    else if (asset is Circle2DST)
                    {
                        circle2DST = (Circle2DST)asset;
                        if (detectionRadiusAT != null)
                        {
                            circle2DST.radiusAttributeType = detectionRadiusAT;
                            EditorUtility.SetDirty(circle2DST);
                        }
                    }
                    else if (asset is MinMaxFloatAT minMaxFloatAT)
                    {
                        if (asset.name == "MovementSpeedAT")
                        {
                            movementSpeedAT = minMaxFloatAT;
                            if (goToBT != null)
                            {
                                goToBT.defaultSelectors = new List<Selector>();
                                Selector selector = new Selector
                                {
                                    attributeType = movementSpeedAT
                                };
                                goToBT.defaultSelectors.Add(selector);
                                EditorUtility.SetDirty(goToBT);
                            }
                        }
                        else if (asset.name == "DetectionRadiusAT")
                        {
                            detectionRadiusAT = minMaxFloatAT;
                            if (circle2DST != null)
                            {
                                circle2DST.radiusAttributeType = detectionRadiusAT;
                                EditorUtility.SetDirty(circle2DST);
                            }
                            else if (sphere3DST != null)
                            {
                                sphere3DST.radiusAttributeType = detectionRadiusAT;
                                EditorUtility.SetDirty(sphere3DST);
                            }
                        }
                    }
                    else if (asset is InventorySlot inventorySlot)
                    {
                        if (inventorySlot.name == inventorySlotNames[2])
                        {
                            inventorySlot.slotType = InventorySlot.SlotType.Invisible;
                            inventorySlot.maxNumberEntities = -1;
                        }
                        else
                        {
                            inventorySlot.slotType = InventorySlot.SlotType.Location;
                            inventorySlot.maxNumberEntities = 1;
                        }

                        EditorUtility.SetDirty(inventorySlot);
                    }

                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Created SO: " + fullPath);
                }
            }
        }

        private void CreateLayers(List<string> layersToCreate)
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = manager.FindProperty("layers");

            foreach (string layer in layersToCreate)
            {
                if (LayerMask.NameToLayer(layer) == -1)
                {
                    for (int i = 8; i < 32; i++)
                    {
                        if (LayerMask.LayerToName(i) == "")
                        {
                            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                            if (sp.stringValue == "")
                            {
                                Debug.Log("Adding " + layer + " to spot " + i);
                                sp.stringValue = layer;
                                manager.ApplyModifiedProperties();
                                break;
                            }
                        }
                    }
                }
            }            
        }

        private void DrawSettings()
        {
            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180;
            EditorGUILayout.BeginVertical(guiStyleBox);

            DrawLogo();

            GUILayout.Label("Total AI Scene Settings", headerStyle);
            GUILayout.Space(8);

            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();

            settingsScrollPosition = EditorGUILayout.BeginScrollView(settingsScrollPosition);

            // Find settings SOs - there can be multiple settings files - we want the one that is set in the Manager
            // Find TotalAIManager in Scene - if doesn't exist tell to go to Setup or Create it
            TotalAIManager[] managers = FindObjectsOfType<TotalAIManager>();
            if (managers.Length == 0)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("No Total AI Manager found in Scene.  Please add one using Setup or manually.", MessageType.Error, true);
            }
            else if (managers.Length > 1)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("Multiple Total AI Managers found in Scene.  Please remove extra managers.", MessageType.Error, true);
            }
            else
            {
                TotalAISettings settings = managers[0].settings;

                if (settings == null)
                {
                    GUILayout.Space(20);
                    EditorGUILayout.HelpBox("Total AI Manager has no Total AI Settings asset.  Please add one using Setup or manually.", MessageType.Error, true);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Settings Location", AssetDatabase.GetAssetPath(settings));
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("The settings asset is loaded from the Total AI Manager that is in the current Scene.");
                    GUILayout.Space(10);

                    float oldLabelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 200f;
                    settings.for2D = EditorGUILayout.Toggle("For 2D", settings.for2D, GUILayout.MaxWidth(600));
                    settings.roundDriveUtility = EditorGUILayout.IntSlider("Round Drive Utility To # Places", settings.roundDriveUtility, 1, 4,
                                                                           GUILayout.MaxWidth(600));
                    settings.scriptableObjectsDirectory = EditorGUILayout.TextField("ScriptableObjects Directory Root", settings.scriptableObjectsDirectory,
                                                                                    GUILayout.MaxWidth(600));
                    settings.prefabsDirectory = EditorGUILayout.TextField("Prefabs Directory Root", settings.prefabsDirectory,
                                                                          GUILayout.MaxWidth(600));
                    settings.driveLevelICT = (InputConditionType)EditorGUILayout.ObjectField("Drive Level ICT", settings.driveLevelICT,
                                                                                             typeof(InputConditionType),
                                                                                             false, GUILayout.MaxWidth(600));
                    settings.driveLevelOCT = (OutputChangeType)EditorGUILayout.ObjectField("Drive Level OCT", settings.driveLevelOCT,
                                                                                           typeof(OutputChangeType),
                                                                                           false, GUILayout.MaxWidth(600));
                    settings.defaultInventoryType = (InventoryType)EditorGUILayout.ObjectField("Default Inventory Type",
                                                                                               settings.defaultInventoryType, typeof(InventoryType),
                                                                                               false, GUILayout.MaxWidth(600));
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Specify each Movement Type that needs to have a NavMesh Surface/Grid/Graph refreshed.");
                    int numMovementTypes = EditorGUILayout.DelayedIntField("Number of Movement Types", settings.movementTypes.Count, GUILayout.MaxWidth(230));
                    while (numMovementTypes != settings.movementTypes.Count)
                    {
                        if (numMovementTypes > settings.movementTypes.Count)
                            settings.movementTypes.Add(null);
                        else
                            settings.movementTypes.RemoveAt(settings.movementTypes.Count - 1);
                    }

                    for (int i = 0; i < settings.movementTypes.Count; i++)
                    {
                        settings.movementTypes[i] = (MovementType)EditorGUILayout.ObjectField("Movement Type # " + (i + 1),
                                                                                               settings.movementTypes[i], typeof(MovementType),
                                                                                               false, GUILayout.MaxWidth(600));
                    }
                    EditorGUIUtility.labelWidth = oldLabelWidth;

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalValue;
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawMatching()
        {

            float originalValue = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 400;
            EditorGUILayout.BeginVertical(guiStyleBox);

            DrawLogo();

            GUILayout.Label("IC to MT/OC Matches", headerStyle);
            GUILayout.Space(8);

            Rect rect = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();

            TotalAIManager[] managers = FindObjectsOfType<TotalAIManager>();
            if (managers.Length == 0)
            {
                GUILayout.Space(20);
                EditorGUILayout.HelpBox("No Total AI Manager found in Scene.  Please add one using Setup or manually.", MessageType.Error, true);
            }
            else
            {
                totalAIManager = managers[0];
                GUILayout.Space(15);
                if (GUILayout.Button("Reset Matches"))
                {
                    totalAIManager.CreateICToMTDictionary();
                }
                GUILayout.Space(15);

                matchingScrollPosition = EditorGUILayout.BeginScrollView(matchingScrollPosition);
                GUILayout.Space(20);
                if (totalAIManager.fixesInputCondition == null)
                    totalAIManager.CreateICToMTDictionary();
                DrawICTMappingTypeMatches();
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = originalValue;
        }

        private void DrawICTMappingTypeMatches()
        {
            if (totalAIManager.fixesInputCondition != null)
            {
                foreach (KeyValuePair<InputCondition, List<TotalAIManager.FixInputCondition>> fixInputConditions in totalAIManager.fixesInputCondition)
                {
                    EditorGUILayout.LabelField(fixInputConditions.Key.ToString(), new GUIStyle(guiStyle) { fontSize = 13, fontStyle = FontStyle.Bold });
                    GUILayout.Space(2f);
                    if (fixInputConditions.Value != null)
                    {
                        foreach (TotalAIManager.FixInputCondition fixInputCondition in fixInputConditions.Value)
                        {
                            EditorGUILayout.LabelField(fixInputCondition.mappingType.name,
                                                       fixInputCondition.mappingType.outputChanges[fixInputCondition.outputChangeIndex].ToString(),
                                                       guiStyle);
                        }
                    }
                    GUILayout.Space(10f);
                }
            }
        }
    }
}
