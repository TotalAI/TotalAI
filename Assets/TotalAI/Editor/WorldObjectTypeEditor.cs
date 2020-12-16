using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Linq;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(WorldObjectType))]
    public class WorldObjectTypeEditor : EntityTypeEditor
    {
        private float lineHeight;
        private GUIStyle infoBoxStyle;

        private ReorderableList prefabList;
        private ReorderableList stateList;
        private ReorderableList outputChangeList;

        private SerializedProperty serializedPrefabs;
        private SerializedProperty serializedStates;
        private SerializedProperty serializedOutputChanges;

        private WorldObjectType worldObjectType;
        
        private Texture2D completeIcon;
        private Texture2D damageIcon;
        private Texture2D statesIcon;
        private Texture2D skinMappingsIcon;
        private Texture2D skinPrefabsIcon;
        private Texture2D recipesIcon;

        public static bool showcompleteInfo;
        public static bool showStates;
        public static bool showdamageInfo;
        public static bool showSkinPrefabs;
        public static bool showRecipes;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            completeIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/build.png", typeof(Texture2D)) as Texture2D;
            damageIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/damage.png", typeof(Texture2D)) as Texture2D;
            statesIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/states.png", typeof(Texture2D)) as Texture2D;
            skinMappingsIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/inventory.png", typeof(Texture2D)) as Texture2D;
            skinPrefabsIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/change.png", typeof(Texture2D)) as Texture2D;
            recipesIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/stack.png", typeof(Texture2D)) as Texture2D;

            worldObjectType = (WorldObjectType)target;

            lineHeight = EditorGUIUtility.singleLineHeight;
            
            serializedPrefabs = serializedObject.FindProperty("skinPrefabMappings");
            prefabList = new ReorderableList(serializedObject, serializedPrefabs, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawPrefabListItems,
                elementHeightCallback = PrefabHeight,
                onAddCallback = OnAddPrefab,
                headerHeight = 1
            };

            serializedStates = serializedObject.FindProperty("states");
            stateList = new ReorderableList(serializedObject, serializedStates, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawStateListItems,
                onAddCallback = OnAddState,
                elementHeightCallback = StateHeight,
                drawHeaderCallback = DrawStateHeader,
            };

            serializedOutputChanges = serializedObject.FindProperty("statesOutputChanges");
            outputChangeList = new ReorderableList(serializedObject, serializedOutputChanges, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawOutputChangeListItems,
                onAddCallback = OnAddOutputChange,
                elementHeightCallback = OutputChangeHeight,
                drawHeaderCallback = DrawOutputChangeHeader,
            };
        }

        private float PrefabHeight(int index)
        {
            SerializedProperty element = prefabList.serializedProperty.GetArrayElementAtIndex(index);

            float itemHeight = lineHeight + 3;

            float height = itemHeight * 4;

            WorldObjectType.SkinPrefabMapping.ChangeType changeType =
                (WorldObjectType.SkinPrefabMapping.ChangeType)element.FindPropertyRelative("changeType").enumValueIndex;

            if (changeType == WorldObjectType.SkinPrefabMapping.ChangeType.ToggleActive)
                height += itemHeight * 2;

            SerializedProperty inventorySlotConditions = element.FindPropertyRelative("inventorySlotConditions");
            if (inventorySlotConditions.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * element.FindPropertyRelative("inventorySlotConditions").arraySize;
            }
            if (worldObjectType.completeType != WorldObjectType.CompleteType.None)
            {
                height += itemHeight;
            }
            if (worldObjectType.canBeDamaged)
            {
                height += itemHeight;
            }
            if (worldObjectType.states != null && worldObjectType.states.Count > 0)
            {
                height += itemHeight;
                SerializedProperty states = element.FindPropertyRelative("states");
                if (states.isExpanded)
                {
                    height += itemHeight;
                    height += itemHeight * element.FindPropertyRelative("states").arraySize;
                }
            }
            return height + 10;

        }

        private void OnAddPrefab(ReorderableList list)
        {
            serializedPrefabs.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("prefab").objectReferenceValue = null;
            element.FindPropertyRelative("changeType").enumValueIndex = 0;
            element.FindPropertyRelative("enableType").enumValueIndex = 0;
            element.FindPropertyRelative("disableType").enumValueIndex = 0;
            element.FindPropertyRelative("childStartIndex").intValue = 0;
            element.FindPropertyRelative("prefabVariantIndex").intValue = -1;
            element.FindPropertyRelative("minComplete").floatValue = -1f;
            element.FindPropertyRelative("maxComplete").floatValue = -1f;
            element.FindPropertyRelative("minDamage").floatValue = -1f;
            element.FindPropertyRelative("maxDamage").floatValue = -1f;
            element.FindPropertyRelative("inventorySlotConditions").arraySize = 0;
            element.FindPropertyRelative("states").arraySize = 0;
        }

        void DrawPrefabListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = prefabList.serializedProperty.GetArrayElementAtIndex(index);
            float itemWidth = EditorGUIUtility.currentViewWidth - 75;
            float itemHeight = lineHeight + 3;
            float currentY = rect.y + 7;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("changeType"));

            WorldObjectType.SkinPrefabMapping.ChangeType changeType =
                (WorldObjectType.SkinPrefabMapping.ChangeType)element.FindPropertyRelative("changeType").enumValueIndex;

            if (changeType == WorldObjectType.SkinPrefabMapping.ChangeType.ReplaceGameObject)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("prefab"));
            }
            else
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("enableType"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("disableType"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("childStartIndex"));
            }

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("prefabVariantIndex"));

            if (worldObjectType.completeType != WorldObjectType.CompleteType.None)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 10, lineHeight),
                                        element.FindPropertyRelative("minComplete"), new GUIContent("Complete Range", "Set both to -1 to ignore"));
                GUI.Label(new Rect(rect.x + itemWidth / 2f + 12, currentY, 10, lineHeight), "-");
                EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 23, currentY, 35, lineHeight),
                                        element.FindPropertyRelative("maxComplete"), GUIContent.none);
            }

            if (worldObjectType.canBeDamaged)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 10, lineHeight),
                                        element.FindPropertyRelative("minDamage"), new GUIContent("Damage Range", "Set both to -1 to ignore"));
                GUI.Label(new Rect(rect.x + itemWidth / 2f + 12, currentY, 10, lineHeight), "-");
                EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 23, currentY, 35, lineHeight),
                                        element.FindPropertyRelative("maxDamage"), GUIContent.none);
            }
            
            currentY += itemHeight;
            SerializedProperty inventorySlotConditions = element.FindPropertyRelative("inventorySlotConditions");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), inventorySlotConditions);

            if (inventorySlotConditions.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                inventorySlotConditions.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                              "How Many Slot Conditions?", inventorySlotConditions.arraySize);

                for (int i = 0; i < inventorySlotConditions.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty inventorySlotCondition = inventorySlotConditions.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            inventorySlotCondition, new GUIContent("Slot Condition #" + (i + 1)));
                }
                EditorGUI.indentLevel--;
            }

            if (worldObjectType.states != null && worldObjectType.states.Count > 0)
            {
                currentY += itemHeight;
                SerializedProperty states = element.FindPropertyRelative("states");
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), states);

                if (states.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    currentY += itemHeight;
                    states.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many States?", states.arraySize);

                    for (int i = 0; i < states.arraySize; i++)
                    {
                        currentY += itemHeight;
                        SerializedProperty state = states.GetArrayElementAtIndex(i);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                state, new GUIContent("State #" + (i + 1)));
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawStateHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, 300f, lineHeight + 5f), "States - Start state set on World Object", headerStyle);
        }

        private void OnAddState(ReorderableList list)
        {
            serializedStates.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("name").stringValue = "";
            element.FindPropertyRelative("allowsCompletion").boolValue = false;
            element.FindPropertyRelative("allowsDamage").boolValue = false;
            element.FindPropertyRelative("isTimed").boolValue = false;
            element.FindPropertyRelative("timeInGameMinutes").boolValue = false;
            element.FindPropertyRelative("secondsUntilNextState").floatValue = 0f;
            element.FindPropertyRelative("gameMinutesUntilNextState").floatValue = 0f;
            element.FindPropertyRelative("nextStateName").stringValue = "";
            element.FindPropertyRelative("hasEnterOutputChanges").boolValue = false;
            element.FindPropertyRelative("enterOutputChangesIndexes").arraySize = 0;
            element.FindPropertyRelative("hasExitOutputChanges").boolValue = false;
            element.FindPropertyRelative("exitOutputChangesIndexes").arraySize = 0;
            element.FindPropertyRelative("minComplete").floatValue = -1f;
            element.FindPropertyRelative("maxComplete").floatValue = -1f;
            element.FindPropertyRelative("minDamage").floatValue = -1f;
            element.FindPropertyRelative("maxDamage").floatValue = -1f;
        }

        private float StateHeight(int index)
        {
            SerializedProperty element = stateList.serializedProperty.GetArrayElementAtIndex(index);
            float itemHeight = lineHeight + 3;
            float height = itemHeight * 4;

            if (element.FindPropertyRelative("isTimed").boolValue)
                height += itemHeight * 3;

            if (worldObjectType.completeType != WorldObjectType.CompleteType.None)
                height += itemHeight * 2;
            if (worldObjectType.canBeDamaged)
                height += itemHeight * 2;

            if (element.FindPropertyRelative("hasEnterOutputChanges").boolValue)
            {
                height += itemHeight;
                SerializedProperty enterOutputChangesIndexes = element.FindPropertyRelative("enterOutputChangesIndexes");
                if (enterOutputChangesIndexes.isExpanded)
                {
                    height += itemHeight;
                    height += itemHeight * element.FindPropertyRelative("enterOutputChangesIndexes").arraySize;
                }
            }

            if (element.FindPropertyRelative("hasExitOutputChanges").boolValue)
            {
                height += itemHeight;
                SerializedProperty exitOutputChangesIndexes = element.FindPropertyRelative("exitOutputChangesIndexes");
                if (exitOutputChangesIndexes.isExpanded)
                {
                    height += itemHeight;
                    height += itemHeight * element.FindPropertyRelative("exitOutputChangesIndexes").arraySize;
                }
            }
            return height + 10;
        }

        void DrawStateListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = stateList.serializedProperty.GetArrayElementAtIndex(index);
            
            float itemWidth = EditorGUIUtility.currentViewWidth - 75;
            float itemHeight = lineHeight + 3;
            float currentY = rect.y + 7;
            
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("name"));

            if (worldObjectType.completeType != WorldObjectType.CompleteType.None)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 30, lineHeight),
                                    element.FindPropertyRelative("allowsCompletion"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 10, lineHeight),
                                        element.FindPropertyRelative("minComplete"), new GUIContent("Complete Range", "Set both to -1 to ignore"));
                GUI.Label(new Rect(rect.x + itemWidth / 2f + 12, currentY, 10, lineHeight), "-");
                EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 23, currentY, 35, lineHeight),
                                        element.FindPropertyRelative("maxComplete"), GUIContent.none);
            }

            if (worldObjectType.canBeDamaged)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 30, lineHeight),
                                        element.FindPropertyRelative("allowsDamage"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 10, lineHeight),
                                        element.FindPropertyRelative("minDamage"), new GUIContent("Damage Range", "Set both to -1 to ignore"));
                GUI.Label(new Rect(rect.x + itemWidth / 2f + 12, currentY, 10, lineHeight), "-");
                EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 23, currentY, 35, lineHeight),
                                        element.FindPropertyRelative("maxDamage"), GUIContent.none);
            }
            
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 30, lineHeight),
                                    element.FindPropertyRelative("isTimed"));
            if (element.FindPropertyRelative("isTimed").boolValue)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 10, lineHeight),
                                        element.FindPropertyRelative("timeInGameMinutes"));
                bool timeInGameMinutes = element.FindPropertyRelative("timeInGameMinutes").boolValue;
                if (!timeInGameMinutes)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 10, lineHeight),
                                            element.FindPropertyRelative("secondsUntilNextState"));
                }
                else
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2 + 10, lineHeight),
                                            element.FindPropertyRelative("gameMinutesUntilNextState"));
                }
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                        element.FindPropertyRelative("nextStateName"));
                EditorGUI.indentLevel--;
            }

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                    element.FindPropertyRelative("hasEnterOutputChanges"));
            if (element.FindPropertyRelative("hasEnterOutputChanges").boolValue)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                SerializedProperty enterOutputChangesIndexes = element.FindPropertyRelative("enterOutputChangesIndexes");
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), enterOutputChangesIndexes);

                if (enterOutputChangesIndexes.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    currentY += itemHeight;
                    enterOutputChangesIndexes.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                                 "How Many Enter OCs?", enterOutputChangesIndexes.arraySize);

                    for (int i = 0; i < enterOutputChangesIndexes.arraySize; i++)
                    {
                        currentY += itemHeight;
                        SerializedProperty enterOutputChangesIndex = enterOutputChangesIndexes.GetArrayElementAtIndex(i);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                enterOutputChangesIndex, new GUIContent("Enter OC Index #" + (i + 1)));
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                    element.FindPropertyRelative("hasExitOutputChanges"));
            if (element.FindPropertyRelative("hasExitOutputChanges").boolValue)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                SerializedProperty exitOutputChangesIndexes = element.FindPropertyRelative("exitOutputChangesIndexes");
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), exitOutputChangesIndexes);

                if (exitOutputChangesIndexes.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    currentY += itemHeight;
                    exitOutputChangesIndexes.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                                 "How Many Exit OCs?", exitOutputChangesIndexes.arraySize);

                    for (int i = 0; i < exitOutputChangesIndexes.arraySize; i++)
                    {
                        currentY += itemHeight;
                        SerializedProperty exitOutputChangesIndex = exitOutputChangesIndexes.GetArrayElementAtIndex(i);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                exitOutputChangesIndex, new GUIContent("Exit OC Index #" + (i + 1)));
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawOutputChangeHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, 200f, lineHeight + 5f), "State Transition Output Changes", headerStyle);
        }

        private void OnAddOutputChange(ReorderableList list)
        {
            serializedOutputChanges.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            EditorUtilities.OnAddOutputChange(element);
        }

        private float OutputChangeHeight(int index)
        {
            SerializedProperty element = outputChangeList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorUtilities.OutputChangeHeight(index, null, element, infoBoxStyle, true, true);
        }

        private void DrawOutputChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = outputChangeList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawOutputChangeItem(index, null, element, rect, infoBoxStyle, true, true);
        }

        public override void OnInspectorGUI()
        {
            infoBoxStyle = EditorStyles.helpBox;
            //infoBoxStyle.border = new RectOffset(10, 10, 10, 10);
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixels(new Color[] { new Color(0.3f, 0.3f, 0.3f) });
            texture.Apply();
            infoBoxStyle.normal.background = texture;
            infoBoxStyle.richText = true;
            infoBoxStyle.wordWrap = true;
            infoBoxStyle.fontSize = 12;
            infoBoxStyle.padding = new RectOffset(20, 20, 5, 5);
            infoBoxStyle.alignment = TextAnchor.MiddleLeft;

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

            string completeTypeString = ((WorldObjectType.CompleteType)serializedObject.FindProperty("completeType").enumValueIndex).ToString();
            string rightInfo = completeTypeString;
            showcompleteInfo = EditorUtilities.BeginInspectorSection("Complete", completeIcon, showcompleteInfo, sectionColor, sectionBackground, rightInfo);
            if (showcompleteInfo)
            {
                // TODO: On Change make sure addItems timing is Created
                EditorGUI.BeginChangeCheck();
                SerializedProperty serializedBuildType = serializedObject.FindProperty("completeType");
                EditorGUILayout.PropertyField(serializedBuildType);
                if (EditorGUI.EndChangeCheck() && serializedBuildType.enumValueIndex == (int)WorldObjectType.CompleteType.None)
                    serializedObject.FindProperty("addDefaultInventoryTiming").enumValueIndex = (int)WorldObjectType.AddInventoryTiming.Created;

                if (serializedBuildType.enumValueIndex == (int)WorldObjectType.CompleteType.Built)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("itemsConsumedPerBuild"));
                else if (serializedBuildType.enumValueIndex == (int)WorldObjectType.CompleteType.Grows)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("growthRate"));

                if (serializedBuildType.enumValueIndex != (int)WorldObjectType.CompleteType.None)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pointsToComplete"));

                    if (serializedObject.FindProperty("skinPrefabMappings").arraySize == 0)
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoScale"));
                }
            }
            EditorUtilities.EndInspectorSection(showcompleteInfo);

            rightInfo = serializedObject.FindProperty("canBeDamaged").boolValue ? "Allowed" : "None";
            showdamageInfo = EditorUtilities.BeginInspectorSection("Damage", damageIcon, showdamageInfo, sectionColor, sectionBackground, rightInfo);
            if (showdamageInfo)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canBeDamaged"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damageToDestroy"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("removeOnFullDamage"));
            }
            EditorUtilities.EndInspectorSection(showdamageInfo);

            rightInfo = "";
            int numStates = serializedObject.FindProperty("states").arraySize;
            for (int i = 0; i < numStates; i++)
            {
                if (serializedObject.FindProperty("states").GetArrayElementAtIndex(i).FindPropertyRelative("name") != null)
                    rightInfo += serializedObject.FindProperty("states").GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue + ", ";
            }
            rightInfo = rightInfo.Length == 0 ? "None" : rightInfo.Substring(0, rightInfo.Length - 2);

            showStates = EditorUtilities.BeginInspectorSection("States", statesIcon, showStates, sectionColor, sectionBackground, rightInfo);
            if (showStates)
            {
                stateList.DoLayoutList();

                bool anyOutputChanges = false;
                SerializedProperty states = serializedObject.FindProperty("states");
                for (int i = 0; i < states.arraySize; i++)
                {
                    SerializedProperty state = states.GetArrayElementAtIndex(i);
                    if (state.FindPropertyRelative("hasEnterOutputChanges").boolValue || state.FindPropertyRelative("hasExitOutputChanges").boolValue)
                    {
                        anyOutputChanges = true;
                        break;
                    }
                }

                if (anyOutputChanges)
                    outputChangeList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showStates);

            int numSkinPrefabs = serializedObject.FindProperty("skinPrefabMappings").arraySize;
            rightInfo = numSkinPrefabs > 0 ? numSkinPrefabs.ToString() : "None";
            showSkinPrefabs = EditorUtilities.BeginInspectorSection("Skin Prefabs", skinPrefabsIcon, showSkinPrefabs, sectionColor,
                                                                    sectionBackground, rightInfo);
            if (showSkinPrefabs)
            {

                EditorGUILayout.PropertyField(serializedObject.FindProperty("skinGameObjectName"));
                GUILayout.Space(10);
                prefabList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showSkinPrefabs);

            int numRecipes = serializedObject.FindProperty("recipes").arraySize;
            rightInfo = numRecipes > 0 ? numRecipes.ToString() : "None";
            showRecipes = EditorUtilities.BeginInspectorSection("Recipes", recipesIcon, showRecipes, sectionColor, sectionBackground, rightInfo);
            if (showRecipes)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("recipes"));
            }
            EditorUtilities.EndInspectorSection(showRecipes);
            
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
    }
}
 