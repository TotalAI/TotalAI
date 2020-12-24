using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TotalAI.Editor
{

    [CustomEditor(typeof(MappingType))]
    public class MappingTypeEditor : UnityEditor.Editor
    {
        private List<EntityType> allEntityTypes;
        private MappingType mappingType;

        private float lineHeight;

        private GUIStyle headerStyle;
        private GUIStyle infoBoxStyle;
        
        private ReorderableList utilityModifierList;
        private ReorderableList targetFactorList;
        private ReorderableList inventoryFactorList;
        private ReorderableList inputConditionList;
        private ReorderableList outputChangeList;

        private SerializedProperty serializedUtilityModifiers;
        private SerializedProperty serializedTargetFactors;
        private SerializedProperty serializedInventoryFactors;
        private SerializedProperty serializedInputConditions;
        private SerializedProperty serializedOutputChanges;

        private void OnEnable()
        {
            allEntityTypes = new List<EntityType>();
            string[] guids = AssetDatabase.FindAssets("t:EntityType");
            foreach (string guid in guids)
            {
                allEntityTypes.Add((EntityType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            mappingType = (MappingType)target;
            lineHeight = EditorGUIUtility.singleLineHeight;
            headerStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };
            headerStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            serializedUtilityModifiers = serializedObject.FindProperty("utilityModifierInfos");
            utilityModifierList = new ReorderableList(serializedObject, serializedUtilityModifiers, true, true, true, true)
            {
                drawElementCallback = DrawUtilityModifierListItems,
                drawHeaderCallback = DrawUtilityModifierHeader,
                onAddCallback = OnAddUtilityModifier
            };

            serializedTargetFactors = serializedObject.FindProperty("targetFactorInfos");
            targetFactorList = new ReorderableList(serializedObject, serializedTargetFactors, true, true, true, true)
            {
                drawElementCallback = DrawTargetFactorListItems,
                drawHeaderCallback = DrawTargetFactorHeader,
                onAddCallback = OnAddTargetFactor
            };

            serializedInventoryFactors = serializedObject.FindProperty("inventoryFactorInfos");
            inventoryFactorList = new ReorderableList(serializedObject, serializedInventoryFactors, true, true, true, true)
            {
                drawElementCallback = DrawInventoryFactorListItems,
                drawHeaderCallback = DrawInventoryFactorHeader,
                onAddCallback = OnAddInventoryFactor
            };

            serializedInputConditions = serializedObject.FindProperty("inputConditions");
            inputConditionList = new ReorderableList(serializedObject, serializedInputConditions, true, true, true, true)
            {
                drawElementCallback = DrawInputConditionListItems,
                drawHeaderCallback = DrawInputConditionHeader,
                onAddCallback = OnAddInputCondition,
                elementHeightCallback = InputConditionHeight
            };

            serializedOutputChanges = serializedObject.FindProperty("outputChanges");
            outputChangeList = new ReorderableList(serializedObject, serializedOutputChanges, true, true, true, true)
            {
                drawElementCallback = DrawOutputChangeListItems,
                drawHeaderCallback = DrawOutputChangeHeader,
                onAddCallback = OnAddOutputChange,
                elementHeightCallback = OutputChangeHeight
            };
        }

        private void DrawUtilityModifierHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Utility Modifier", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 350, rect.y + 1, 100, lineHeight + 5), "Weight", headerStyle);
        }

        private void OnAddUtilityModifier(ReorderableList list)
        {
            serializedUtilityModifiers.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("utilityModifier").objectReferenceValue = null;
            element.FindPropertyRelative("weight").floatValue = 1;
        }

        private void DrawUtilityModifierListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = utilityModifierList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("utilityModifierInfos");

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 325, lineHeight),
                                    element.FindPropertyRelative("utilityModifier"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 335, rect.y, 40, lineHeight),
                                    element.FindPropertyRelative("weight"), GUIContent.none);
        }

        private void DrawTargetFactorHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Target Factors", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 350, rect.y + 1, 100, lineHeight + 5), "Weight", headerStyle);
        }

        private void OnAddTargetFactor(ReorderableList list)
        {
            serializedTargetFactors.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("targetFactor").objectReferenceValue = null;
            element.FindPropertyRelative("weight").floatValue = 1;
        }

        private void DrawTargetFactorListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = targetFactorList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("targetFactor");
            TargetFactor targetFactor = null;

            if (typeSerialized != null)
            {
                targetFactor = (TargetFactor)typeSerialized.objectReferenceValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 325, lineHeight),
                                    element.FindPropertyRelative("targetFactor"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < serializedTargetFactors.arraySize - 1; i++)
                {
                    if (i != index && serializedTargetFactors.GetArrayElementAtIndex(i).FindPropertyRelative("targetFactor").objectReferenceValue ==
                        element.FindPropertyRelative("targetFactor").objectReferenceValue)
                    {
                        element.FindPropertyRelative("targetFactor").objectReferenceValue = null;
                        EditorUtility.DisplayDialog("That Target Factor has already been added", "Please select a different Target Factor.", "OK");
                        break;
                    }
                }

            }

            EditorGUI.PropertyField(new Rect(rect.x + 335, rect.y, 40, lineHeight),
                                    element.FindPropertyRelative("weight"), GUIContent.none);
        }


        private void DrawInventoryFactorHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Inventory Target Factors", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 350, rect.y + 1, 100, lineHeight + 5), "Weight", headerStyle);
        }

        private void OnAddInventoryFactor(ReorderableList list)
        {
            serializedInventoryFactors.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("targetFactor").objectReferenceValue = null;
            element.FindPropertyRelative("weight").floatValue = 1;
        }

        private void DrawInventoryFactorListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = inventoryFactorList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty typeSerialized = element.FindPropertyRelative("targetFactor");
            TargetFactor targetFactor = null;

            if (typeSerialized != null)
            {
                targetFactor = (TargetFactor)typeSerialized.objectReferenceValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 325, lineHeight),
                                    element.FindPropertyRelative("targetFactor"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < serializedTargetFactors.arraySize - 1; i++)
                {
                    if (i != index && serializedTargetFactors.GetArrayElementAtIndex(i).FindPropertyRelative("targetFactor").objectReferenceValue ==
                        element.FindPropertyRelative("targetFactor").objectReferenceValue)
                    {
                        element.FindPropertyRelative("targetFactor").objectReferenceValue = null;
                        EditorUtility.DisplayDialog("That Target Factor has already been added", "Please select a different Target Factor.", "OK");
                        break;
                    }
                }

            }

            EditorGUI.PropertyField(new Rect(rect.x + 335, rect.y, 40, lineHeight),
                                    element.FindPropertyRelative("weight"), GUIContent.none);
        }

        private void DrawInputConditionHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, lineHeight + 5), "Input Conditions", headerStyle);
        }

        private void OnAddInputCondition(ReorderableList list)
        {
            serializedInputConditions.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            EditorUtilities.OnAddInputCondition(element);
        }

        private float InputConditionHeight(int index)
        {
            SerializedProperty element = inputConditionList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorUtilities.InputConditionHeight(mappingType, element, index, infoBoxStyle);
        }

        private void DrawInputConditionListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = inputConditionList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawInputConditionItem(mappingType, element, index, rect, infoBoxStyle);
        }

        private void DrawOutputChangeHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1f, 200f, lineHeight + 5f), "Output Changes", headerStyle);
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
            return EditorUtilities.OutputChangeHeight(index, mappingType, element, infoBoxStyle);
        }

        private void DrawOutputChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = outputChangeList.serializedProperty.GetArrayElementAtIndex(index);
            EditorUtilities.DrawOutputChangeItem(index, mappingType, element, rect, infoBoxStyle);
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

            GUI.enabled = false;
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
            GUI.enabled = true;

            ActionType oldActionType = (ActionType)serializedObject.FindProperty("actionType").objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("actionType"));
            if (EditorGUI.EndChangeCheck())
            {
                // On Change add this MappingType to the ActionType's mappingTypes list
                ActionType newActionType = (ActionType)serializedObject.FindProperty("actionType").objectReferenceValue;
                if (newActionType != null)
                {
                    if (newActionType.mappingTypes == null)
                        newActionType.mappingTypes = new List<MappingType>();
                    newActionType.mappingTypes.Add(mappingType);
                    mappingType.selectors = new List<Selector>();
                    EditorUtility.SetDirty(mappingType);
                    EditorUtility.SetDirty(newActionType);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    if (oldActionType != null)
                    {
                        oldActionType.mappingTypes.Remove(mappingType);
                        EditorUtility.SetDirty(oldActionType);
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawActionTypeSelectors("actionType", "selectors", "overrideDefaultSelectors",
                                    mappingType.overrideDefaultSelectors, mappingType.selectors);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("goToActionType"));

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawActionTypeSelectors("goToActionType", "goToSelectors", "overrideDefaultGoToSelectors",
                                    mappingType.overrideDefaultGoToSelectors, mappingType.goToSelectors);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("filterOptions"));
            if (serializedObject.FindProperty("filterOptions").enumValueIndex == 1)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blacklistAgentTypes"));
            else if (serializedObject.FindProperty("filterOptions").enumValueIndex == 2)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("whitelistAgentTypes"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistanceAsInput"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entityTargetSearchRadius"));
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reevaluateTargets"));

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            utilityModifierList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            targetFactorList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            inventoryFactorList.DoLayoutList();
            
            // TODO: Turn this into a button that will show matches
            /*
            if (!mappingType.AnyEntityTypeMatchesTypeGroups(allEntityTypes))
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No EntityTypes Match All ICs Type Groups.", MessageType.Error);
            }
            */
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            inputConditionList.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            outputChangeList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
            //DrawDefaultInspector();
        }

        private void DrawActionTypeSelectors(string actionTypeName, string attributeSelectorsName, string overrideDefaultsName,
                                                      List<bool> overrideDefaults, List<Selector> attributeSelectors)
        {
            ActionType actionType = (ActionType)serializedObject.FindProperty(actionTypeName).objectReferenceValue;

            if (actionType != null)
            {
                SerializedProperty serializedOverrideDefaults = serializedObject.FindProperty(overrideDefaultsName);
                SerializedProperty serializedAttributeSelectors = serializedObject.FindProperty(attributeSelectorsName);

                // Use the BehaviorType's EditorHelp to figure out what AttributeValueSelectors are needed
                BehaviorType behaviorType = actionType.behaviorType;
                if (behaviorType == null)
                {
                    EditorGUILayout.HelpBox("The " + actionTypeName + " selected is missing a BehaviorType.  A BehaviorType is needed before " +
                                            "selecting the Selectors.", MessageType.Info);
                }
                else if (behaviorType.editorHelp.valueTypes != null && behaviorType.editorHelp.valueTypes.Length > 0)
                {
                    BehaviorType.EditorHelp editorHelp = behaviorType.editorHelp;
                    if (editorHelp.valueTypes.Length != behaviorType.defaultSelectors.Count)
                    {
                        EditorGUILayout.HelpBox("The " + actionTypeName + "'s behaviorType (" + behaviorType.name + ") is missing Default Selectors.  " +
                                                "Please fix the BehaviorType.", MessageType.Error);
                    }
                    else if (attributeSelectors == null || attributeSelectors.Count != editorHelp.valueTypes.Length)
                    {
                        // TODO: A popup warning that these are about to be cleared if they exist

                        overrideDefaults = new List<bool>();
                        attributeSelectors = new List<Selector>();
                        for (int i = 0; i < editorHelp.valueTypes.Length; i++)
                        {
                            overrideDefaults.Add(false);
                            attributeSelectors.Add(new Selector());
                        }
                        if (actionTypeName == "actionType")
                        {
                            mappingType.overrideDefaultSelectors = overrideDefaults;
                            mappingType.selectors = attributeSelectors;
                        }
                        else
                        {
                            mappingType.overrideDefaultGoToSelectors = overrideDefaults;
                            mappingType.goToSelectors = attributeSelectors;
                        }
                        
                        EditorUtility.SetDirty(mappingType);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        for (int i = 0; i < editorHelp.valueTypes.Length; i++)
                        {
                            SerializedProperty sp = serializedAttributeSelectors.GetArrayElementAtIndex(i);
                            SerializedProperty spOverrides = serializedOverrideDefaults.GetArrayElementAtIndex(i);

                            EditorGUILayout.LabelField(actionType.name + ": " + editorHelp.valueDescriptions[i], headerStyle);

                            string name;
                            string selectionValue = "";
                            Selector selector = behaviorType.defaultSelectors[i];
                            switch (selector.valueType)
                            {
                                case Selector.ValueType.AttributeType:
                                    name = selector.attributeType != null ? selector.attributeType.name : "None";
                                    if (selector.selectionType == Selector.SelectionType.FixedValue)
                                    {
                                        EnumAT enumAT = selector.attributeType as EnumAT;
                                        if (enumAT != null)
                                            selectionValue = " - " + enumAT.OptionNames()[(int)selector.fixedValue];
                                        else
                                            selectionValue = " - " + selector.fixedValue;
                                    }
                                    else if (selector.selectionType == Selector.SelectionType.SelectorType)
                                    {
                                        selectionValue = " - " + selector.selectorType.name;
                                    }
                                    EditorGUILayout.LabelField("Default: Attribute Type = " + name + " (" + selector.selectionType + selectionValue + ")");
                                    break;
                                case Selector.ValueType.MinMax:
                                    name = selector.minMax != null ? selector.minMax.name : "None";
                                    if (selector.selectionType == Selector.SelectionType.FixedValue)
                                    {
                                        selectionValue = " - " + selector.fixedValue;
                                    }
                                    else if (selector.selectionType == Selector.SelectionType.SelectorType)
                                    {
                                        selectionValue = " - " + selector.selectorType.name;
                                    }
                                    EditorGUILayout.LabelField("Default: Min Max = " + name + " (" + selector.selectionType + selectionValue + ")");
                                    break;
                                case Selector.ValueType.Choices:
                                    name = selector.choices != null ? selector.choices.name : "None";
                                    if (selector.selectionType == Selector.SelectionType.FixedValue)
                                    {
                                       selectionValue = " - " + selector.choices.OptionNames()[(int)selector.fixedValue];
                                    }
                                    else if (selector.selectionType == Selector.SelectionType.SelectorType)
                                    {
                                        selectionValue = " - " + selector.selectorType.name;
                                    }
                                    EditorGUILayout.LabelField("Default: Choices = " + name + " (" + selector.selectionType + selectionValue + ")");
                                    break;
                            }
                            
                            EditorGUILayout.PropertyField(spOverrides, new GUIContent("Override Default"));

                            if (spOverrides.boolValue)
                            {
                                DrawSelector(sp, editorHelp.valueTypes[i]);
                            }
                            GUILayout.Space(10);
                        }
                    }
                }
            }
            else
            {
                // No actionType or goToActionType set - clear these if they exist
                if (actionTypeName == "actionType")
                {
                    if (mappingType.selectors != null && mappingType.selectors.Count > 0)
                    {
                        mappingType.selectors = null;
                        mappingType.overrideDefaultSelectors = null;

                        EditorUtility.SetDirty(mappingType);
                        AssetDatabase.SaveAssets();
                    }
                }
                else
                {
                    if (mappingType.goToSelectors != null && mappingType.goToSelectors.Count > 0)
                    {
                        mappingType.goToSelectors = null;
                        mappingType.overrideDefaultGoToSelectors = null;

                        EditorUtility.SetDirty(mappingType);
                        AssetDatabase.SaveAssets();
                    }
                }
                
                EditorGUILayout.HelpBox(actionTypeName + " must be selected before selecting the Selectors.", MessageType.Info, true);
            }
        }

        private void DrawSelector(SerializedProperty sp, Type type)
        {
            EditorGUILayout.PropertyField(sp.FindPropertyRelative("valueType"));
            int valueType = sp.FindPropertyRelative("valueType").enumValueIndex;
            switch (valueType)
            {
                case (int)Selector.ValueType.AttributeType:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("attributeType"), type);
                    break;
                case (int)Selector.ValueType.MinMax:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("minMax"), typeof(MinMax));
                    break;
                case (int)Selector.ValueType.Choices:
                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("choices"), typeof(Choices));
                    break;
            }

            // TODO:  If its Choices with a ChoiceType (dynamic) - Fixed can't be selected since we don't know the choices
            // Need to remove option from selectionType

            EditorGUILayout.PropertyField(sp.FindPropertyRelative("selectionType"));
            switch (sp.FindPropertyRelative("selectionType").enumValueIndex)
            {
                case (int)Selector.SelectionType.FixedValue:
                    // Show the field based on the attributeType
                    if (valueType == (int)Selector.ValueType.AttributeType)
                    {
                        AttributeType attributeType = (AttributeType)sp.FindPropertyRelative("attributeType").objectReferenceValue;
                        if (attributeType != null)
                        {
                            attributeType.DrawFixedValueField(sp);
                        }
                    }
                    else if (valueType == (int)Selector.ValueType.MinMax)
                    {
                        MinMax minMax = (MinMax)sp.FindPropertyRelative("minMax").objectReferenceValue;
                        if (minMax != null)
                        {
                            EditorGUILayout.PropertyField(sp.FindPropertyRelative("fixedValue"));
                        }
                    }
                    else
                    {
                        Choices choices = (Choices)sp.FindPropertyRelative("choices").objectReferenceValue;
                        if (choices != null)
                        {
                            choices.DrawFixedValueField(sp);
                        }
                    }
                    break;
                case (int)Selector.SelectionType.SelectorType:

                    EditorGUILayout.ObjectField(sp.FindPropertyRelative("selectorType"));
                    break;
            }
        }
    }
}