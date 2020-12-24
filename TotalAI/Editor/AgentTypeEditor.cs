using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace TotalAI.Editor
{
    [CustomEditor(typeof(AgentType))]
    public class AgentTypeEditor : EntityTypeEditor
    {
        private AgentType agentType;

        private float lineHeight;
        GUIStyle infoBoxStyle;

        private ReorderableList attributeList;
        private ReorderableList actionList;
        private ReorderableList driveList;
        private ReorderableList animatorOverridesList;

        private SerializedProperty serializedDefaultAttributes;
        private SerializedProperty serializedDefaultActions;
        private SerializedProperty serializedDefaultDrives;
        private SerializedProperty serializedAnimatorOverrides;

        public static bool showCoreTypes;
        public static bool showDriveTypes;
        public static bool showActionTypes;
        public static bool showAttributeTypes;
        public static bool showAnimatorOverrides;

        private Texture2D driveIcon;
        private Texture2D actionIcon;
        private Texture2D overrideIcon;
        private Texture2D attributeIcon;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            overrideIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/override.png", typeof(Texture2D)) as Texture2D;
            actionIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/action.png", typeof(Texture2D)) as Texture2D;
            driveIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/drive.png", typeof(Texture2D)) as Texture2D;
            attributeIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/attribute.png", typeof(Texture2D)) as Texture2D;

            lineHeight = EditorGUIUtility.singleLineHeight;

            agentType = (AgentType)target;

            serializedDefaultAttributes = serializedObject.FindProperty("defaultAttributes");
            attributeList = new ReorderableList(serializedObject, serializedDefaultAttributes, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawAttributeListItems,
                drawHeaderCallback = DrawAttributeHeader,
                onAddCallback = OnAddAttribute
            };

            serializedDefaultActions = serializedObject.FindProperty("defaultActions");
            actionList = new ReorderableList(serializedObject, serializedDefaultActions, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawActionListItems,
                drawHeaderCallback = DrawActionHeader,
                onAddCallback = OnAddAction
            };

            serializedDefaultDrives = serializedObject.FindProperty("defaultDrives");
            driveList = new ReorderableList(serializedObject, serializedDefaultDrives, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawDriveListItems,
                elementHeightCallback = DriveHeight,
                drawHeaderCallback = DrawDriveHeader,
                onAddCallback = OnAddDrive
            };

            serializedAnimatorOverrides = serializedObject.FindProperty("animatorOverridesConditions"); ;
            animatorOverridesList = new ReorderableList(serializedObject, serializedAnimatorOverrides, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawInputConditionListItems,
                elementHeightCallback = InputConditionHeight,
                drawHeaderCallback = DrawInputConditionHeader,
                onAddCallback = OnAddInputCondition
            };
        }

        private void DrawInputConditionHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, 20 + 5), "Animation Overrides Conditions", headerStyle);
        }

        private void OnAddInputCondition(ReorderableList list)
        {
            serializedAnimatorOverrides.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            EditorUtilities.OnAddInputCondition(element);
        }

        private float InputConditionHeight(int index)
        {
            SerializedProperty element = animatorOverridesList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorUtilities.InputConditionHeight(null, element, index, infoBoxStyle);
        }

        private void DrawInputConditionListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = animatorOverridesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawInputConditionItem(null, element, index, rect, infoBoxStyle);
        }

        private void OnAddAttribute(ReorderableList list)
        {
            serializedDefaultAttributes.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("type").objectReferenceValue = null;
        }

        void DrawAttributeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = attributeList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("type");
            AttributeType attributeType = null;

            if (typeSerialized != null)
            {
                attributeType = (AttributeType)typeSerialized.objectReferenceValue;
            }

            // On Change make sure AttributeType is not already in the list
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 225, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("type"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < serializedDefaultAttributes.arraySize - 1; i++)
                {
                    if (i != index && serializedDefaultAttributes.GetArrayElementAtIndex(i).FindPropertyRelative("type").objectReferenceValue ==
                        element.FindPropertyRelative("type").objectReferenceValue)
                    {
                        element.FindPropertyRelative("type").objectReferenceValue = null;
                        EditorUtility.DisplayDialog("That Attribute Type has already been added", "Please select a different Attribute Type.", "OK");
                        break;
                    }
                }
                
            }

            if (attributeType != null)
            {
                attributeType.DrawDefaultValueFields(rect, element);
            }
        }

        void DrawAttributeHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 250, rect.y + 1, 50, EditorGUIUtility.singleLineHeight + 5), "Value", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 300, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Range", headerStyle);
        }

        private void OnAddAction(ReorderableList list)
        {
            serializedDefaultActions.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("actionType").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
            element.FindPropertyRelative("changeProbability").floatValue = 0;
            element.FindPropertyRelative("changeAmount").floatValue = 0;
        }

        void DrawActionListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = actionList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("actionType");
            ActionType actionType = null;

            if (typeSerialized != null)
            {
                actionType = (ActionType)typeSerialized.objectReferenceValue;
            }

            // On Change make sure AttributeType is not already in the list
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 225, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("actionType"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < serializedDefaultActions.arraySize - 1; i++)
                {
                    if (i != index && serializedDefaultActions.GetArrayElementAtIndex(i).FindPropertyRelative("actionType").objectReferenceValue ==
                        element.FindPropertyRelative("actionType").objectReferenceValue)
                    {
                        element.FindPropertyRelative("actionType").objectReferenceValue = null;
                        EditorUtility.DisplayDialog("That Action Type has already been added", "Please select a different Action Type.", "OK");
                        break;
                    }
                }

            }

            EditorGUI.PropertyField(new Rect(rect.x + 235, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("level"), GUIContent.none);
            
            EditorGUI.PropertyField(new Rect(rect.x + 290, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("changeProbability"), GUIContent.none);
                
            EditorGUI.PropertyField(new Rect(rect.x + 345, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("changeAmount"), GUIContent.none);
        }

        void DrawActionHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 250, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Level", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 305, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Prob", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 360, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Change Amt", headerStyle);

        }

        private void OnAddDrive(ReorderableList list)
        {
            serializedDefaultDrives.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("driveType").objectReferenceValue = null;
            element.FindPropertyRelative("startingLevel").floatValue = 0;
            element.FindPropertyRelative("changePerGameHour").floatValue = 0;
            element.FindPropertyRelative("maxTimeCurve").floatValue = 0;
            element.FindPropertyRelative("minTimeCurve").floatValue = 0;
        }

        private float DriveHeight(int index)
        {
            SerializedProperty element = driveList.serializedProperty.GetArrayElementAtIndex(index);

            float itemHeight = lineHeight + 3;
            float height = itemHeight;
            
            if (element.FindPropertyRelative("overrideDriveType").boolValue)
            {
                height += itemHeight;
            }
            return height;
        }

        void DrawDriveListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = driveList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("driveType");
            DriveType driveType = null;

            if (typeSerialized != null)
            {
                driveType = (DriveType)typeSerialized.objectReferenceValue;
            }
            
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("driveType"), GUIContent.none);

            if (driveType != null)
            {
                EditorGUI.PropertyField(new Rect(rect.x + 210, rect.y, 30, EditorGUIUtility.singleLineHeight),
                                            element.FindPropertyRelative("startingLevel"), GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + 250, rect.y, 40, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("overrideDriveType"), GUIContent.none);

                if (driveType.changeType == DriveType.RateOfChangeType.Constant)
                    GUI.Label(new Rect(rect.x + 275, rect.y, 50, EditorGUIUtility.singleLineHeight), driveType.changePerGameHour.ToString());
                else
                    GUI.Label(new Rect(rect.x + 275, rect.y, 50, EditorGUIUtility.singleLineHeight),
                              driveType.minTimeCurve + " - " + driveType.maxTimeCurve);

                GUI.Label(new Rect(rect.x + 325, rect.y, 200, EditorGUIUtility.singleLineHeight), driveType.changeType.ToString());

                if (element.FindPropertyRelative("overrideDriveType").boolValue)
                {
                    float y = rect.y + lineHeight + 3;
                    if (driveType.changeType == DriveType.RateOfChangeType.Constant)
                    {
                        GUI.Label(new Rect(rect.x, y, 200, EditorGUIUtility.singleLineHeight), "Change Per Game Hour");
                        EditorGUI.PropertyField(new Rect(rect.x + 150, y, 40, EditorGUIUtility.singleLineHeight),
                                                element.FindPropertyRelative("changePerGameHour"), GUIContent.none);
                    }
                    else
                    {
                        GUI.Label(new Rect(rect.x, y, 50, EditorGUIUtility.singleLineHeight), "Min");
                        EditorGUI.PropertyField(new Rect(rect.x + 30, y, 30, EditorGUIUtility.singleLineHeight),
                                                element.FindPropertyRelative("minTimeCurve"), GUIContent.none);
                        GUI.Label(new Rect(rect.x + 70, y, 50, EditorGUIUtility.singleLineHeight), "Max");
                        EditorGUI.PropertyField(new Rect(rect.x + 105, y, 30, EditorGUIUtility.singleLineHeight),
                                                element.FindPropertyRelative("maxTimeCurve"), GUIContent.none);
                        GUI.Label(new Rect(rect.x + 145, y, 50, EditorGUIUtility.singleLineHeight), "Curve");
                        EditorGUI.PropertyField(new Rect(rect.x + 190, y, 230, EditorGUIUtility.singleLineHeight),
                                                element.FindPropertyRelative("rateTimeCurve"), GUIContent.none);
                    }


                }
            }


        }

        void DrawDriveHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 14, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Drive Type", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 225, rect.y + 1, 50, EditorGUIUtility.singleLineHeight + 5), "Level", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 265, rect.y + 1, 150, EditorGUIUtility.singleLineHeight + 5), "Override Defaults", headerStyle);
            //EditorGUI.LabelField(new Rect(rect.x + 305, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Change Per Game Hour", headerStyle);
        }

        public override void OnInspectorGUI()
        {
            infoBoxStyle = EditorStyles.helpBox;
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

            if (MissingAnyCoreTypes(serializedObject))
                showCoreTypes = true;

            string rightInfo;
            showCoreTypes = EditorUtilities.BeginInspectorSection("Core Types", personIcon, showCoreTypes, sectionColor, sectionBackground);
            if (showCoreTypes)
            {
                if (MissingAnyCoreTypes(serializedObject))
                {
                    GUILayout.Space(2);
                    EditorGUILayout.HelpBox("Missing Required Types.  Please Fix.", MessageType.Error);
                    showCoreTypes = true;
                    GUILayout.Space(5);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultMovementType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultAnimationType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useAnimatorOverrides"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("idleLayer"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("idleState"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultDeciderType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultMemoryType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultHistoryType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultUtilityFunction"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultNoPlansMappingType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultNoPlansDriveType"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNumDetectedEntities"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSensorTypes"));
            }
            EditorUtilities.EndInspectorSection(showCoreTypes);
            int numDriveTypes = serializedObject.FindProperty("defaultDrives").arraySize;
            rightInfo = numDriveTypes > 0 ? numDriveTypes.ToString() : "None";
            showDriveTypes = EditorUtilities.BeginInspectorSection("Drive Types", driveIcon, showDriveTypes, sectionColor, sectionBackground, rightInfo);
            if (showDriveTypes)
            {
                driveList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showDriveTypes);

            int numActionTypes = serializedObject.FindProperty("defaultActions").arraySize;
            rightInfo = numActionTypes > 0 ? numActionTypes.ToString() : "None";
            showActionTypes = EditorUtilities.BeginInspectorSection("Action Types", actionIcon, showActionTypes, sectionColor, sectionBackground, rightInfo);
            if (showActionTypes)
            {
                actionList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showActionTypes);

            int numAttributes = serializedObject.FindProperty("defaultAttributes").arraySize;
            rightInfo = numAttributes > 0 ? numAttributes.ToString() : "None";
            showAttributeTypes = EditorUtilities.BeginInspectorSection("Attribute Types", attributeIcon, showAttributeTypes,
                                                                       sectionColor, sectionBackground, rightInfo);
            if (showAttributeTypes)
            {
                attributeList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showAttributeTypes);

            SerializedProperty animatorOverrides = serializedObject.FindProperty("animatorOverrides");
            rightInfo = animatorOverrides.arraySize > 0 ? animatorOverrides.arraySize.ToString() : "None";
            showAnimatorOverrides = EditorUtilities.BeginInspectorSection("Animator Overrides", overrideIcon, showAnimatorOverrides,
                                                                          sectionColor, sectionBackground, rightInfo);
            if (showAnimatorOverrides)
            {
                animatorOverrides.arraySize = EditorGUILayout.DelayedIntField("How Many Overrides?", animatorOverrides.arraySize);
                GUILayout.Space(10f);
                EditorGUI.indentLevel++;
                for (int i = 0; i < animatorOverrides.arraySize; i++)
                {
                    GUILayout.Label("Animator Override #" + (i + 1), new GUIStyle("boldLabel"));
                    SerializedProperty animatorOverride = animatorOverrides.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(animatorOverride.FindPropertyRelative("controller"));

                    SerializedProperty conditionIndexes = animatorOverride.FindPropertyRelative("conditionIndexes");
                    conditionIndexes.arraySize = EditorGUILayout.DelayedIntField("How Many Conditions?", conditionIndexes.arraySize);
                    for (int j = 0; j < conditionIndexes.arraySize; j++)
                    {
                        SerializedProperty conditionIndex = conditionIndexes.GetArrayElementAtIndex(j);
                        EditorGUILayout.PropertyField(conditionIndex, new GUIContent("Condition Index #" + (j + 1)));
                    }
                    GUILayout.Space(5f);
                }
                EditorGUI.indentLevel--;

                GUILayout.Space(15f);
                animatorOverridesList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showAnimatorOverrides);
            
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

            Repaint();
        }
        
        private bool MissingAnyCoreTypes(SerializedObject serializedObject)
        {
            if (serializedObject.FindProperty("defaultMovementType").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultAnimationType").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultDeciderType").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultMemoryType").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultHistoryType").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultUtilityFunction").objectReferenceValue == null)
                return true;
            if (serializedObject.FindProperty("defaultMovementType").objectReferenceValue == null)
                return true;
            return false;
        }
    }
}
