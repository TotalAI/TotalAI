using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TotalAI
{
    public static class EditorUtilities
    {

        public static readonly Color L0 = new Color(18f / 255f, 18f / 255f, 18f / 255f);
        public static readonly Color L1 = new Color(33f / 255f, 33f / 255f, 33f / 255f);
        public static readonly Color L15 = new Color(45f / 255f, 45f / 255f, 45f / 255f);
        public static readonly Color L2 = new Color(66f / 255f, 66f / 255f, 66f / 255f);
        public static readonly Color L3 = new Color(97f / 255f, 97f / 255f, 97f / 255f);

        public static bool[] outputChangesMoreFoldout = new bool[50];

        public static bool BeginInspectorSection(string title, Texture2D icon, bool show, Color backgroundColor,
                                                 Texture2D background, string rightInfo = null)
        {
            Color defaultColor = GUI.backgroundColor;

            GUI.backgroundColor = backgroundColor;
            Rect r2 = EditorGUILayout.BeginVertical(new GUIStyle("helpBox")
            {
                padding = new RectOffset(0, 0, 1, 1),
                margin = new RectOffset(7, 7, 7, 7)
            });
            GUI.backgroundColor = defaultColor;
            
            Rect rect = EditorGUILayout.BeginHorizontal();
            int leftPadding = icon == null ? 15 : 40;
            GUIStyle style = new GUIStyle("helpBox")
            {
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(leftPadding, 0, 10, 10),
                margin = new RectOffset(0, 0, 1, 1),
                fontSize = 13
            };
            
            style.normal.background = background;
            style.hover.background = Texture2D.whiteTexture;
            
            GUI.backgroundColor = backgroundColor;
            bool toggle = GUILayout.Button(title, style);
            GUI.backgroundColor = defaultColor;
            if (toggle)
                show = !show;

            if (icon != null)
                GUI.Button(new Rect(rect.x + 10, rect.y, 20, rect.height), icon, new GUIStyle("label"));

            if (rightInfo != null)
                GUI.Label(new Rect(rect.x + 100, rect.y, rect.width - 110, rect.height), rightInfo,
                    new GUIStyle("label")
                    {
                        alignment = TextAnchor.MiddleRight,
                        fontSize = 13
                    });

            EditorGUILayout.EndHorizontal();
            if (show)
            {
                GUIStyle style2 = new GUIStyle("helpBox")
                {
                    stretchWidth = true,
                    padding = new RectOffset(15, 5, 10, 10),
                    margin = new RectOffset(0, 0, 2, 1)
                };
                style2.normal.background = background;

                GUI.backgroundColor = backgroundColor;
                EditorGUILayout.BeginVertical(style2);
                GUI.backgroundColor = defaultColor;
            }

            return show;
        }

        public static void EndInspectorSection(bool show)
        {
            if (show)
            {
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }

        public static void DrawReordableListBackground(Rect rect, int index, bool isActive, bool isFocused)
        {

            Texture2D whiteTexture = Texture2D.whiteTexture;
            GUIStyle style = new GUIStyle("box") { padding = new RectOffset(10, 10, 10, 10) };
            style.normal.background = whiteTexture;

            Color defaultColor = GUI.backgroundColor;
            if (isActive || isFocused)
                GUI.backgroundColor = new Color(1f, 1f, 1f, .08f);
            else
                GUI.backgroundColor = new Color(1f, 1f, 1f, .04f);

            GUI.Box(rect, GUIContent.none, style);
            GUI.backgroundColor = defaultColor;
        }

        public static void OnAddInputCondition(SerializedProperty element)
        {
            element.FindPropertyRelative("inputConditionType").objectReferenceValue = null;
            element.FindPropertyRelative("matchType").enumValueIndex = 0;
            element.FindPropertyRelative("typeGroupMatch").objectReferenceValue = null;
            element.FindPropertyRelative("typeCategoryMatch").objectReferenceValue = null;
            element.FindPropertyRelative("entityTypeMatch").objectReferenceValue = null;
            element.FindPropertyRelative("inventoryMatchType").enumValueIndex = 0;
            element.FindPropertyRelative("inventoryTypeGroupMatch").objectReferenceValue = null;
            element.FindPropertyRelative("inventoryTypeCategoryMatch").objectReferenceValue = null;
            element.FindPropertyRelative("inventoryEntityTypeMatch").objectReferenceValue = null;
            element.FindPropertyRelative("sharesInventoryTypeGroupWith").intValue = 0;
            element.FindPropertyRelative("entityType").objectReferenceValue = null;
            element.FindPropertyRelative("levelType").objectReferenceValue = null;
            element.FindPropertyRelative("floatValue").floatValue = 0;
            element.FindPropertyRelative("intValue").intValue = 0;
            element.FindPropertyRelative("boolValue").boolValue = false;
            element.FindPropertyRelative("max").floatValue = -1f;
            element.FindPropertyRelative("min").floatValue = -1f;
            element.FindPropertyRelative("stringValue").stringValue = "";
            element.FindPropertyRelative("actionSkillCurve").objectReferenceValue = null;
        }

        public static float InputConditionHeight(MappingType mappingType, SerializedProperty element, int index, GUIStyle infoBoxStyle)
        {
            SerializedProperty typeSerialized = element.FindPropertyRelative("inputConditionType");
            InputConditionType inputConditionType = null;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            inputConditionType = (InputConditionType)typeSerialized.objectReferenceValue;
            if (inputConditionType != null)
            {
                float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
                float itemHeight = lineHeight + 3f;

                float height = itemHeight * 2f;
                if (inputConditionType.typeInfo.description != "")
                {
                    float labelHeight = infoBoxStyle.CalcHeight(new GUIContent(inputConditionType.typeInfo.description), itemWidth);
                    height += labelHeight + 16f;
                }

                if (inputConditionType.typeInfo.usesTypeGroup)
                    height += itemHeight * 2;
                if (inputConditionType.typeInfo.usesTypeGroupFromIndex)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesInventoryTypeGroup)
                    height += itemHeight * 2;
                if (inputConditionType.typeInfo.usesTypeGroup && inputConditionType.typeInfo.usesInventoryTypeGroup)
                    height += itemHeight;

                if (inputConditionType.typeInfo.usesInventoryTypeGroupShareWith)
                {
                    int matchIndex = element.FindPropertyRelative("sharesInventoryTypeGroupWith").intValue;
                    int numICs = mappingType.inputConditions.Count;
                    if (matchIndex < 0 || matchIndex >= numICs || matchIndex == index ||
                        !mappingType.inputConditions[matchIndex].inputConditionType.typeInfo.usesInventoryTypeGroup)
                    {
                        height += itemHeight * 3;
                    }
                    height += itemHeight;
                }

                if (inputConditionType.typeInfo.usesLevelType)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesLevelTypes)
                {
                    height += itemHeight;
                    SerializedProperty levelTypes = element.FindPropertyRelative("levelTypes");
                    if (levelTypes.isExpanded)
                    {
                        height += itemHeight;
                        height += itemHeight * element.FindPropertyRelative("levelTypes").arraySize;
                    }
                }

                if (inputConditionType.typeInfo.usesEntityType)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesFloatValue)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesEnumValue)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesMinMax)
                    height += itemHeight * 2f;
                if (inputConditionType.typeInfo.usesBoolValue)
                    height += itemHeight;
                if (inputConditionType.typeInfo.usesStringValue)
                    height += itemHeight;

                if (mappingType == null)
                    height -= itemHeight;

                return height + 20f;
            }

            return lineHeight + 20f;
        }

        public static void DrawInputConditionItem(MappingType mappingType, SerializedProperty element, int index, Rect rect, GUIStyle infoBoxStyle)
        {
            SerializedProperty typeSerialized = element.FindPropertyRelative("inputConditionType");
            InputConditionType inputConditionType = null;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            if (typeSerialized != null)
            {
                inputConditionType = (InputConditionType)typeSerialized.objectReferenceValue;
            }
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;
            GUIContent label;
            if (inputConditionType != null)
            {
                label = new GUIContent("Input Condition Type", inputConditionType.typeInfo.inputConditionTypeTooltip);

                if (inputConditionType.typeInfo.description != "")
                {
                    float labelHeight = infoBoxStyle.CalcHeight(new GUIContent(inputConditionType.typeInfo.description), itemWidth);
                    EditorGUI.LabelField(new Rect(rect.x, currentY, itemWidth, labelHeight + 10), inputConditionType.typeInfo.description, infoBoxStyle);
                    currentY += labelHeight + 16f;
                }
            }
            else
            {
                label = new GUIContent("Input Condition Type");
            }

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                element.FindPropertyRelative("inputConditionType"), label);

            if (inputConditionType != null)
            {
                if (mappingType != null)
                {
                    currentY += itemHeight;
                    string canBeFixedBy;
                    if (inputConditionType.matchingOCTs == null || inputConditionType.matchingOCTs.Count == 0)
                    {
                        canBeFixedBy = "Nothing";
                    }
                    else
                    {
                        canBeFixedBy = inputConditionType.matchingOCTs.Aggregate("", (current, s) => current + (s.name + ", "));
                        canBeFixedBy = canBeFixedBy.Remove(canBeFixedBy.Length - 2, 2);
                    }
                    EditorGUI.LabelField(new Rect(rect.x, currentY, itemWidth, lineHeight), "Can Be Fixed By", canBeFixedBy);
                }

                if (inputConditionType.typeInfo.usesTypeGroup)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 100f, lineHeight),
                        element.FindPropertyRelative("matchType"));
                    int matchType = element.FindPropertyRelative("matchType").enumValueIndex;

                    if (matchType == (int)InputCondition.MatchType.TypeGroup)
                    {
                        UnityEngine.Object typeGroup = element.FindPropertyRelative("typeGroupMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Type Group Match");
                        typeGroup = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, typeGroup,
                                                          typeof(TypeGroup), false);
                        element.FindPropertyRelative("typeGroupMatch").objectReferenceValue = typeGroup;
                    }
                    else if (matchType == (int)InputCondition.MatchType.TypeCategory)
                    {
                        UnityEngine.Object typeCategory = element.FindPropertyRelative("typeCategoryMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Type Category Match");
                        typeCategory = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, typeCategory,
                                                             typeof(TypeCategory), false);
                        element.FindPropertyRelative("typeCategoryMatch").objectReferenceValue = typeCategory;
                    }
                    else
                    {
                        UnityEngine.Object entityTypeMatch = element.FindPropertyRelative("entityTypeMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Entity Type Match");
                        entityTypeMatch = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, entityTypeMatch,
                                                             typeof(EntityType), false);
                        element.FindPropertyRelative("entityTypeMatch").objectReferenceValue = entityTypeMatch;
                    }
                }

                if (inputConditionType.typeInfo.usesTypeGroupFromIndex)
                {
                    currentY += itemHeight;
                    EditorGUI.LabelField(new Rect(rect.x, currentY, itemWidth, lineHeight), "Type Group Match",
                                         "From Other Input Conditions");
                }

                if (inputConditionType.typeInfo.usesInventoryTypeGroup)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 100f, lineHeight),
                        element.FindPropertyRelative("inventoryMatchType"));
                    int matchType = element.FindPropertyRelative("inventoryMatchType").enumValueIndex;

                    if (matchType == (int)InputCondition.MatchType.TypeGroup)
                    {
                        UnityEngine.Object inventoryTypeGroup = element.FindPropertyRelative("inventoryTypeGroupMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Inventory Type Group Match");
                        inventoryTypeGroup = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, inventoryTypeGroup,
                                                          typeof(TypeGroup), false);
                        element.FindPropertyRelative("inventoryTypeGroupMatch").objectReferenceValue = inventoryTypeGroup;
                    }
                    else if (matchType == (int)InputCondition.MatchType.TypeCategory)
                    {
                        UnityEngine.Object inventoryTypeCategory = element.FindPropertyRelative("inventoryTypeCategoryMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Inventory Type Category Match");
                        inventoryTypeCategory = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, inventoryTypeCategory,
                                                             typeof(TypeCategory), false);
                        element.FindPropertyRelative("inventoryTypeCategoryMatch").objectReferenceValue = inventoryTypeCategory;
                    }
                    else
                    {
                        UnityEngine.Object inventoryEntityTypeMatch = element.FindPropertyRelative("inventoryEntityTypeMatch").objectReferenceValue;
                        currentY += itemHeight;
                        GUIContent label2 = new GUIContent("Inventory Entity Type Match");
                        inventoryEntityTypeMatch = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), label2, inventoryEntityTypeMatch,
                                                             typeof(EntityType), false);
                        element.FindPropertyRelative("inventoryEntityTypeMatch").objectReferenceValue = inventoryEntityTypeMatch;
                    }
                }

                if (inputConditionType.typeInfo.usesInventoryTypeGroupShareWith)
                {
                    int matchIndex = element.FindPropertyRelative("sharesInventoryTypeGroupWith").intValue;
                    int numICs = mappingType.inputConditions.Count;
                    if (matchIndex < 0 || matchIndex >= numICs || matchIndex == index ||
                        !mappingType.inputConditions[matchIndex].inputConditionType.typeInfo.usesInventoryTypeGroup)
                    {
                        currentY += itemHeight;
                        EditorGUI.HelpBox(new Rect(rect.x, currentY, itemWidth, lineHeight * 3),
                                          "Shares Inventory Type Group With points to an Invalid InputCondition.", MessageType.Error);
                        currentY += itemHeight * 2;
                    }

                    GUIContent sharesLabel = new GUIContent("Shares Inventory Type Group With",
                                                           "What InputCondition index does this share the same Inventory Group Type with?  If different " +
                                                           "it will take the intersection of the matches.");
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("sharesInventoryTypeGroupWith"), sharesLabel);
                }

                if (inputConditionType.typeInfo.usesLevelType)
                {
                    UnityEngine.Object levelType = element.FindPropertyRelative("levelType").objectReferenceValue;
                    currentY += itemHeight;
                    levelType = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      ObjectNames.NicifyVariableName(inputConditionType.typeInfo.mostRestrictiveLevelType.Name),
                                                      levelType, inputConditionType.typeInfo.mostRestrictiveLevelType, false);
                    element.FindPropertyRelative("levelType").objectReferenceValue = levelType;
                }

                if (inputConditionType.typeInfo.usesLevelTypes)
                {
                    int numLevelTypes = element.FindPropertyRelative("levelTypes").arraySize;

                    currentY += itemHeight;
                    SerializedProperty levelTypes = element.FindPropertyRelative("levelTypes");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), levelTypes);

                    if (levelTypes.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        currentY += itemHeight;
                        levelTypes.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                                     "How Many?", levelTypes.arraySize);
                        string levelTypeLabel = ObjectNames.NicifyVariableName(inputConditionType.typeInfo.mostRestrictiveLevelType.Name);

                        for (int i = 0; i < levelTypes.arraySize; i++)
                        {
                            currentY += itemHeight;
                            Object levelType = levelTypes.GetArrayElementAtIndex(i).objectReferenceValue;
                            levelType = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), levelTypeLabel + " #" + (i + 1),
                                                      levelType, inputConditionType.typeInfo.mostRestrictiveLevelType, false);
                            levelTypes.GetArrayElementAtIndex(i).objectReferenceValue = levelType;
                        }
                        EditorGUI.indentLevel--;
                    }
                }

                if (inputConditionType.typeInfo.usesEntityType)
                {
                    UnityEngine.Object entityType = element.FindPropertyRelative("entityType").objectReferenceValue;
                    currentY += itemHeight;
                    entityType = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      ObjectNames.NicifyVariableName(inputConditionType.typeInfo.mostRestrictiveEntityType.Name),
                                                      entityType, inputConditionType.typeInfo.mostRestrictiveEntityType, false);
                    element.FindPropertyRelative("entityType").objectReferenceValue = entityType;
                }
                if (inputConditionType.typeInfo.usesEnumValue)
                {
                    currentY += itemHeight;
                    string[] optionNames = inputConditionType.typeInfo.enumType.GetEnumNames();
                    for (int i = 0; i < optionNames.Length; i++)
                    {
                        optionNames[i] = ObjectNames.NicifyVariableName(optionNames[i]);
                    }

                    int[] optionValues = (int[])inputConditionType.typeInfo.enumType.GetEnumValues();

                    element.FindPropertyRelative("enumValueIndex").intValue =
                        EditorGUI.IntPopup(new Rect(rect.x, currentY, itemWidth / 2f + 150f, lineHeight),
                                           ObjectNames.NicifyVariableName(inputConditionType.typeInfo.enumType.Name),
                                           element.FindPropertyRelative("enumValueIndex").intValue, optionNames, optionValues);
                }
                if (inputConditionType.typeInfo.usesFloatValue)
                {
                    GUIContent floatLabel = new GUIContent(inputConditionType.typeInfo.floatLabel);
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("floatValue"), floatLabel);
                }
                if (inputConditionType.typeInfo.usesMinMax)
                {
                    GUIContent minLabel = new GUIContent(inputConditionType.typeInfo.minLabel);
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("min"), minLabel);
                    GUIContent maxLabel = new GUIContent(inputConditionType.typeInfo.maxLabel);
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("max"), maxLabel);
                }
                if (inputConditionType.typeInfo.usesBoolValue)
                {
                    GUIContent boolLabel = new GUIContent(inputConditionType.typeInfo.boolLabel);
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                        element.FindPropertyRelative("boolValue"), boolLabel);
                }
                if (inputConditionType.typeInfo.usesStringValue)
                {
                    GUIContent stringLabel = new GUIContent(inputConditionType.typeInfo.stringLabel);
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 100f, lineHeight),
                                        element.FindPropertyRelative("stringValue"), stringLabel);
                }
            }
        }

        public static void OnAddOutputChange(SerializedProperty element)
        {
            element.FindPropertyRelative("outputChangeType").objectReferenceValue = null;
            element.FindPropertyRelative("levelType").objectReferenceValue = null;
            element.FindPropertyRelative("entityType").objectReferenceValue = null;
            element.FindPropertyRelative("targetType").enumValueIndex = 0;
            element.FindPropertyRelative("timing").enumValueIndex = 0;
            element.FindPropertyRelative("gameMinutes").floatValue = 0;
            element.FindPropertyRelative("onAnimationEventOccurence").intValue = 0;
            element.FindPropertyRelative("valueType").enumValueIndex = 0;
            element.FindPropertyRelative("boolValue").boolValue = false;
            element.FindPropertyRelative("floatValue").floatValue = 0;
            element.FindPropertyRelative("enumValueIndex").intValue = 0;
            element.FindPropertyRelative("stringValue").stringValue = "";
            element.FindPropertyRelative("actionSkillCurve").objectReferenceValue = null;
            element.FindPropertyRelative("unityObject").objectReferenceValue = null;
            element.FindPropertyRelative("changeConditions").arraySize = 0;
            element.FindPropertyRelative("stopType").enumValueIndex = 0;
            element.FindPropertyRelative("blockMakeChangeForcedStop").boolValue = false;
            element.FindPropertyRelative("recheckInputConditionsIndexes").arraySize = 0;
            element.FindPropertyRelative("stopOnRecheckFail").boolValue = false;
        }

        public static float OutputChangeHeight(int index, MappingType mappingType, SerializedProperty element, GUIStyle infoBoxStyle,
                                               bool hideTargetType = false, bool hideTiming = false)
        {
            SerializedProperty typeSerialized = element.FindPropertyRelative("outputChangeType");
            OutputChangeType outputChangeType = null;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            outputChangeType = (OutputChangeType)typeSerialized.objectReferenceValue;
            if (outputChangeType != null)
            {
                float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
                float itemHeight = lineHeight + 3f;

                float height = itemHeight * 11f;
                if (outputChangeType.typeInfo.description != "")
                {
                    float labelHeight = infoBoxStyle.CalcHeight(new GUIContent(outputChangeType.typeInfo.description), itemWidth);
                    height += labelHeight + 16f;
                }
                if (!hideTiming)
                {
                    if (element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.Repeating)
                        height += itemHeight * 2;
                    if (element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.AfterGameMinutes ||
                        element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.OnAnimationEvent)
                        height += itemHeight;
                }
                else
                {
                    height -= itemHeight;
                }
                int valueTypeIndex = element.FindPropertyRelative("valueType").enumValueIndex;
                if (valueTypeIndex == (int)OutputChange.ValueType.OppPrevOutputAmount ||
                    valueTypeIndex == (int)OutputChange.ValueType.PrevOutputAmount ||
                    valueTypeIndex == (int)OutputChange.ValueType.None)
                    height -= itemHeight;
                else if (valueTypeIndex == (int)OutputChange.ValueType.Selector)
                {
                    SerializedProperty selectionType = element.FindPropertyRelative("selector").FindPropertyRelative("selectionType");
                    if (selectionType.enumValueIndex == (int)Selector.SelectionType.CurrentOrDefaultValue)
                    {
                        height += itemHeight * 2;
                    }
                    else
                    {
                        height += itemHeight * 3;
                    }
                }

                if (!hideTargetType)
                {
                    OutputChange.TargetType targetType = (OutputChange.TargetType)element.FindPropertyRelative("targetType").enumValueIndex;
                    if (outputChangeType.typeInfo.usesInventoryTypeGroupMatchIndex || targetType == OutputChange.TargetType.ToInventoryTarget)
                    {
                        height += itemHeight;

                        int matchIndex = element.FindPropertyRelative("inventoryTypeGroupMatchIndex").intValue;
                        int numICs = mappingType.inputConditions.Count;
                        if (matchIndex < 0 || matchIndex >= numICs ||
                            (mappingType.inputConditions[matchIndex].inputConditionType != null && 
                             !mappingType.inputConditions[matchIndex].inputConditionType.typeInfo.usesInventoryTypeGroup))
                        {
                            height += itemHeight * 3;
                        }
                    }
                }
                else
                {
                    height -= itemHeight;
                }
                if (outputChangeType.typeInfo.usesEntityType)
                    height += itemHeight;
                if (outputChangeType.typeInfo.usesLevelType)
                    height += itemHeight;
                if (outputChangeType.typeInfo.usesBoolValue)
                    height += itemHeight;
                if (outputChangeType.typeInfo.usesFloatValue)
                    height += itemHeight;
                if (outputChangeType.typeInfo.usesStringValue)
                    height += itemHeight;
                if (outputChangeType.typeInfo.usesSelector)
                {
                    SerializedProperty selectionType = element.FindPropertyRelative("selector").FindPropertyRelative("selectionType");
                    if (selectionType.enumValueIndex == (int)Selector.SelectionType.CurrentOrDefaultValue)
                    {
                        height += itemHeight * 4;
                    }
                    else
                    {
                        height += itemHeight * 5;
                    }
                }

                if (outputChangesMoreFoldout[index])
                {
                    SerializedProperty changeConditions = element.FindPropertyRelative("changeConditions");
                    if (changeConditions.isExpanded)
                    {
                        height += itemHeight;
                        height += itemHeight * element.FindPropertyRelative("changeConditions").arraySize;
                    }

                    SerializedProperty recheckInputConditionsIndexes = element.FindPropertyRelative("recheckInputConditionsIndexes");
                    if (recheckInputConditionsIndexes.isExpanded)
                    {
                        height += itemHeight;
                        height += itemHeight * element.FindPropertyRelative("recheckInputConditionsIndexes").arraySize;
                    }
                }
                else
                {
                    height -= itemHeight * 5;
                }


                return height + 20f;
            }

            return lineHeight + 20f;
        }

        public static void DrawOutputChangeItem(int index, MappingType mappingType, SerializedProperty element, Rect rect, GUIStyle infoBoxStyle,
                                                bool hideTargetType = false, bool hideTiming = false)
        {
            SerializedProperty typeSerialized = element.FindPropertyRelative("outputChangeType");
            OutputChangeType outputChangeType = null;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            if (typeSerialized != null)
            {
                outputChangeType = (OutputChangeType)typeSerialized.objectReferenceValue;
            }
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;
            GUIContent label;
            if (outputChangeType != null)
            {
                label = new GUIContent("Output Change Type", "Defines the logic and required values for this Output Change.");

                if (outputChangeType.typeInfo.description != "")
                {
                    float labelHeight = infoBoxStyle.CalcHeight(new GUIContent(outputChangeType.typeInfo.description), itemWidth);
                    EditorGUI.LabelField(new Rect(rect.x, currentY, itemWidth, labelHeight + 10), outputChangeType.typeInfo.description, infoBoxStyle);
                    currentY += labelHeight + 16f;
                }
            }
            else
            {
                label = new GUIContent("Output Change Type");
            }
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                element.FindPropertyRelative("outputChangeType"), label);
            if (EditorGUI.EndChangeCheck())
            {
                outputChangeType = (OutputChangeType)typeSerialized.objectReferenceValue;
                if (outputChangeType != null)
                {
                    // Move enums to first valid options
                    element.FindPropertyRelative("targetType").enumValueIndex = (int)outputChangeType.typeInfo.possibleTargetTypes[0];
                    element.FindPropertyRelative("timing").enumValueIndex = (int)outputChangeType.typeInfo.possibleTimings[0];
                    element.FindPropertyRelative("valueType").enumValueIndex = (int)outputChangeType.typeInfo.possibleValueTypes[0];
                }
            }

            if (outputChangeType != null)
            {
                if (outputChangeType.typeInfo.usesEntityType)
                {
                    UnityEngine.Object entityType = element.FindPropertyRelative("entityType").objectReferenceValue;
                    currentY += itemHeight;
                    GUIContent label2 = new GUIContent(ObjectNames.NicifyVariableName(outputChangeType.typeInfo.entityTypeLabel));
                    entityType = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                            label2,
                                                            entityType, outputChangeType.typeInfo.mostRestrictiveEntityType, false);
                    element.FindPropertyRelative("entityType").objectReferenceValue = entityType;
                }

                if (outputChangeType.typeInfo.usesLevelType)
                {
                    UnityEngine.Object levelType = element.FindPropertyRelative("levelType").objectReferenceValue;
                    currentY += itemHeight;
                    GUIContent label2 = new GUIContent(ObjectNames.NicifyVariableName(outputChangeType.typeInfo.levelTypeLabel));
                    levelType = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                            label2,
                                                            levelType, outputChangeType.typeInfo.mostRestrictiveLevelType, false);
                    element.FindPropertyRelative("levelType").objectReferenceValue = levelType;
                }

                if (!hideTargetType)
                {
                    currentY += itemHeight;
                    OutputChange.TargetType targetType = (OutputChange.TargetType)element.FindPropertyRelative("targetType").enumValueIndex;
                    using (new EditorGUI.DisabledScope(outputChangeType.typeInfo.possibleTargetTypes.Length == 1))
                    {
                        targetType = (OutputChange.TargetType)EditorGUI.EnumPopup(new Rect(rect.x, currentY, itemWidth / 2f + 120f, lineHeight),
                                                                                  new GUIContent("Target Type"), targetType,
                                                                                  outputChangeType.CheckEnabledTargetType);
                    }
                    element.FindPropertyRelative("targetType").enumValueIndex = (int)targetType;

                    if (outputChangeType.typeInfo.usesInventoryTypeGroupMatchIndex || targetType == OutputChange.TargetType.ToInventoryTarget)
                    {
                        int matchIndex = element.FindPropertyRelative("inventoryTypeGroupMatchIndex").intValue;
                        int numICs = mappingType.inputConditions.Count;
                        if (numICs > 0 && (matchIndex < 0 || matchIndex >= numICs ||
                            (mappingType.inputConditions[matchIndex].inputConditionType != null &&
                             !mappingType.inputConditions[matchIndex].inputConditionType.typeInfo.usesInventoryTypeGroup)))
                        {
                            currentY += itemHeight;
                            EditorGUI.HelpBox(new Rect(rect.x, currentY, itemWidth, lineHeight * 3),
                                              "Inventory Type Group Match Index points to an Invalid InputCondition.", MessageType.Error);
                            currentY += itemHeight * 2;
                        }

                        currentY += itemHeight;
                        GUIContent inventoryTypeGroupLabel = new GUIContent("Inventory Type Group Match Index");
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("inventoryTypeGroupMatchIndex"), inventoryTypeGroupLabel);
                    }
                }
                else if ((OutputChange.TargetType)element.FindPropertyRelative("targetType").enumValueIndex != OutputChange.TargetType.ToEntityTarget)
                {
                    // TODO: This default value should be passed in - currently being used in WOT state transitions
                    element.FindPropertyRelative("targetType").enumValueIndex = (int)OutputChange.TargetType.ToEntityTarget;
                }

                if (!hideTiming)
                {
                    currentY += itemHeight;
                    using (new EditorGUI.DisabledScope(outputChangeType.typeInfo.possibleTimings.Length == 1))
                    {
                        OutputChange.Timing timing = (OutputChange.Timing)element.FindPropertyRelative("timing").enumValueIndex;
                        timing = (OutputChange.Timing)EditorGUI.EnumPopup(new Rect(rect.x, currentY, itemWidth / 2f + 120f, lineHeight),
                                                                                  new GUIContent("Timing"), timing,
                                                                                  outputChangeType.CheckEnabledTiming);
                        element.FindPropertyRelative("timing").enumValueIndex = (int)timing;
                    }

                    if (element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.Repeating ||
                        element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.AfterGameMinutes)
                    {
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                            element.FindPropertyRelative("gameMinutes"));
                    }

                    if (element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.Repeating)
                    {
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                            element.FindPropertyRelative("changeEstimateForPlanner"));
                    }

                    if (element.FindPropertyRelative("timing").enumValueIndex == (int)OutputChange.Timing.OnAnimationEvent)
                    {
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                            element.FindPropertyRelative("onAnimationEventOccurence"));
                    }
                }

                currentY += itemHeight;
                OutputChange.ValueType valueType;
                using (new EditorGUI.DisabledScope(outputChangeType.typeInfo.possibleValueTypes.Length == 1))
                {
                    valueType = (OutputChange.ValueType)element.FindPropertyRelative("valueType").enumValueIndex;
                    valueType = (OutputChange.ValueType)EditorGUI.EnumPopup(new Rect(rect.x, currentY, itemWidth / 2f + 200f, lineHeight),
                                                                            new GUIContent("Value Type To Use"), valueType,
                                                                            outputChangeType.CheckEnabledValueType);
                    element.FindPropertyRelative("valueType").enumValueIndex = (int)valueType;
                }

                switch (valueType)
                {
                    case OutputChange.ValueType.BoolValue:
                        currentY += itemHeight;
                        GUIContent boolLabel = new GUIContent(outputChangeType.typeInfo.boolLabel);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("boolValue"), boolLabel);
                        break;
                    case OutputChange.ValueType.FloatValue:
                        currentY += itemHeight;
                        GUIContent floatLabel = new GUIContent(outputChangeType.typeInfo.floatLabel);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("floatValue"), floatLabel);
                        break;
                    case OutputChange.ValueType.IntValue:
                        currentY += itemHeight;
                        GUIContent intLabel = new GUIContent(outputChangeType.typeInfo.intLabel);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                            element.FindPropertyRelative("intValue"), intLabel);
                        break;
                    case OutputChange.ValueType.StringValue:
                        currentY += itemHeight;
                        GUIContent stringLabel = new GUIContent(outputChangeType.typeInfo.stringLabel);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 150f, lineHeight),
                                            element.FindPropertyRelative("stringValue"), stringLabel);
                        break;
                    case OutputChange.ValueType.EnumValue:

                        break;
                    case OutputChange.ValueType.ActionSkillCurve:
                        currentY += itemHeight;
                        GUIContent actionSkillCurveLabel = new GUIContent(outputChangeType.typeInfo.actionSkillCurveLabel);
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            element.FindPropertyRelative("actionSkillCurve"), actionSkillCurveLabel);
                        break;
                    case OutputChange.ValueType.UnityObject:
                        currentY += itemHeight;
                        GUIContent unityObjectLabel = new GUIContent(outputChangeType.typeInfo.unityObjectLabel);
                        UnityEngine.Object unityObject = element.FindPropertyRelative("unityObject").objectReferenceValue;
                        unityObject = EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight), unityObjectLabel,
                                                            unityObject, outputChangeType.typeInfo.unityObjectType, false);
                        element.FindPropertyRelative("unityObject").objectReferenceValue = unityObject;
                        break;
                    case OutputChange.ValueType.Selector:
                        currentY += itemHeight;
                        SerializedProperty sp = element.FindPropertyRelative("selector");
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("valueType"));

                        switch (sp.FindPropertyRelative("valueType").enumValueIndex)
                        {
                            case (int)Selector.ValueType.AttributeType:
                                currentY += itemHeight;
                                EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      sp.FindPropertyRelative("attributeType"), typeof(AttributeType));
                                break;
                            case (int)Selector.ValueType.MinMax:
                                currentY += itemHeight;
                                EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      sp.FindPropertyRelative("minMax"), typeof(MinMax));
                                break;
                            case (int)Selector.ValueType.Choices:
                                currentY += itemHeight;
                                EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      sp.FindPropertyRelative("choices"), typeof(Choices));
                                break;
                        }

                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("selectionType"));

                        switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
                        {
                            case (int)Selector.SelectionType.FixedValue:
                                int selectorValueType = sp.FindPropertyRelative("valueType").enumValueIndex;

                                currentY += itemHeight;
                                // Show the field based on the attributeType
                                AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                if (selectorValueType == (int)Selector.ValueType.AttributeType)
                                {
                                    AttributeType selectorAttributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                    if (selectorAttributeType != null)
                                    {
                                        selectorAttributeType.DrawFixedValueField(sp, new Rect(rect.x, currentY, itemWidth, lineHeight));
                                    }
                                }
                                else if (selectorValueType == (int)Selector.ValueType.MinMax)
                                {
                                    MinMax minMax = (MinMax)sp.FindPropertyRelative("minMax").objectReferenceValue;
                                    if (minMax != null)
                                    {
                                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("fixedValue"));
                                    }
                                }
                                else
                                {
                                    Choices choices = (Choices)sp.FindPropertyRelative("choices").objectReferenceValue;
                                    if (choices != null)
                                    {
                                        choices.DrawFixedValueField(sp, new Rect(rect.x, currentY, itemWidth, lineHeight));
                                    }
                                }
                                break;
                            case (int)Selector.SelectionType.SelectorType:
                                currentY += itemHeight;
                                EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                      sp.FindPropertyRelative("selectorType"));
                                break;
                        }
                        break;
                }

                if (outputChangeType.typeInfo.usesBoolValue)
                {
                    currentY += itemHeight;
                    GUIContent boolLabel = new GUIContent(outputChangeType.typeInfo.boolLabel);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                        element.FindPropertyRelative("boolValue"), boolLabel);
                }
                if (outputChangeType.typeInfo.usesFloatValue)
                {
                    currentY += itemHeight;
                    GUIContent floatLabel = new GUIContent(outputChangeType.typeInfo.floatLabel);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                        element.FindPropertyRelative("floatValue"), floatLabel);
                }
                if (outputChangeType.typeInfo.usesIntValue)
                {
                    currentY += itemHeight;
                    GUIContent intLabel = new GUIContent(outputChangeType.typeInfo.intLabel);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 30f, lineHeight),
                                        element.FindPropertyRelative("intValue"), intLabel);
                }
                if (outputChangeType.typeInfo.usesStringValue)
                {
                    currentY += itemHeight;
                    GUIContent stringLabel = new GUIContent(outputChangeType.typeInfo.stringLabel);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f + 150f, lineHeight),
                                        element.FindPropertyRelative("stringValue"), stringLabel);
                }
                if (outputChangeType.typeInfo.usesSelector)
                {
                    currentY += itemHeight;
                    EditorGUI.LabelField(new Rect(rect.x, currentY, itemWidth, lineHeight), outputChangeType.typeInfo.selectorLabel);
                    EditorGUI.indentLevel++;
                    currentY += itemHeight;
                    SerializedProperty sp = element.FindPropertyRelative("selector");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("valueType"));

                    switch (sp.FindPropertyRelative("valueType").enumValueIndex)
                    {
                        case (int)Selector.ValueType.AttributeType:
                            currentY += itemHeight;
                            EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                  sp.FindPropertyRelative("attributeType"), typeof(AttributeType));
                            break;
                        case (int)Selector.ValueType.MinMax:
                            currentY += itemHeight;
                            EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                  sp.FindPropertyRelative("minMax"), typeof(MinMax));
                            break;
                        case (int)Selector.ValueType.Choices:
                            currentY += itemHeight;
                            EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                  sp.FindPropertyRelative("choices"), typeof(Choices));
                            break;
                    }
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("selectionType"));

                    switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
                    {
                        case (int)Selector.SelectionType.FixedValue:
                            int selectorValueType = sp.FindPropertyRelative("valueType").enumValueIndex;

                            currentY += itemHeight;
                            // Show the field based on the attributeType
                            AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                            if (selectorValueType == (int)Selector.ValueType.AttributeType)
                            {
                                AttributeType selectorAttributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                                if (selectorAttributeType != null)
                                {
                                    selectorAttributeType.DrawFixedValueField(sp, new Rect(rect.x, currentY, itemWidth, lineHeight));
                                }
                            }
                            else if (selectorValueType == (int)Selector.ValueType.MinMax)
                            {
                                MinMax minMax = (MinMax)sp.FindPropertyRelative("minMax").objectReferenceValue;
                                if (minMax != null)
                                {
                                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), sp.FindPropertyRelative("fixedValue"));
                                }
                            }
                            else
                            {
                                Choices choices = (Choices)sp.FindPropertyRelative("choices").objectReferenceValue;
                                if (choices != null)
                                {
                                    choices.DrawFixedValueField(sp, new Rect(rect.x, currentY, itemWidth, lineHeight));
                                }
                            }
                            break;
                        case (int)Selector.SelectionType.SelectorType:
                            currentY += itemHeight;
                            EditorGUI.ObjectField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                  sp.FindPropertyRelative("selectorType"));
                            break;
                    }
                    EditorGUI.indentLevel--;
                }

                currentY += itemHeight;
                outputChangesMoreFoldout[index] = EditorGUI.Foldout(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                             outputChangesMoreFoldout[index], "More Options", true);
                if (outputChangesMoreFoldout[index])
                {
                    EditorGUI.indentLevel++;
                    currentY += itemHeight;
                    SerializedProperty changeConditions = element.FindPropertyRelative("changeConditions");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), changeConditions);

                    if (changeConditions.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        currentY += itemHeight;
                        changeConditions.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                                     "How Many CCs?", changeConditions.arraySize);

                        for (int i = 0; i < changeConditions.arraySize; i++)
                        {
                            currentY += itemHeight;
                            SerializedProperty changeCondition = changeConditions.GetArrayElementAtIndex(i);
                            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                    changeCondition, new GUIContent("CC #" + (i + 1)));
                        }
                        EditorGUI.indentLevel--;
                    }
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            element.FindPropertyRelative("stopType"));
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            element.FindPropertyRelative("blockMakeChangeForcedStop"));

                    currentY += itemHeight;
                    SerializedProperty recheckInputConditionsIndexes = element.FindPropertyRelative("recheckInputConditionsIndexes");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), recheckInputConditionsIndexes);

                    if (recheckInputConditionsIndexes.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        currentY += itemHeight;
                        recheckInputConditionsIndexes.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                                            "How Many IC Indexes?", recheckInputConditionsIndexes.arraySize);

                        for (int i = 0; i < recheckInputConditionsIndexes.arraySize; i++)
                        {
                            currentY += itemHeight;
                            SerializedProperty recheckInputConditionsIndex = recheckInputConditionsIndexes.GetArrayElementAtIndex(i);
                            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                    recheckInputConditionsIndex, new GUIContent("IC Index #" + (i + 1)));
                        }
                        EditorGUI.indentLevel--;
                    }

                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            element.FindPropertyRelative("stopOnRecheckFail"));
                    EditorGUI.indentLevel--;
                }
                EditorGUI.EndFoldoutHeaderGroup();
            }
        }

    }
}