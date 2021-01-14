using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace TotalAI.Editor
{
    public class EntityTypeEditor : InputOutputTypeEditor
    {
        public GUIStyle headerStyle;

        private ReorderableList typeCategoriesList;
        private SerializedProperty serializedTypeCategories;

        private ReorderableList prefabVariantsList;
        private SerializedProperty serializedPrefabVariants;

        private ReorderableList entityTriggersList;
        private SerializedProperty serializedEntityTriggers;

        private ReorderableList defaultTagsList;
        private SerializedProperty serializedDefaultTags;

        private ReorderableList canBeInInventorySlotsList;
        private SerializedProperty serializedCanBeInInventorySlots;

        private ReorderableList transformMappingsList;
        private SerializedProperty serializedTransformMappings;

        private ReorderableList itemAttributeTypeModifiersList;
        private SerializedProperty serializedItemAttributeTypeModifiers;

        private ReorderableList itemActionSkillModifiersList;
        private SerializedProperty serializedItemActionSkillModifiers;

        private ReorderableList itemSoundEffectsList;
        private SerializedProperty serializedItemSoundEffects;

        private ReorderableList itemVisualEffectsList;
        private SerializedProperty serializedItemVisualEffects;

        private ReorderableList inventorySlotsList;
        private SerializedProperty serializedInventorySlots;

        private ReorderableList defaultInventoryList;
        private SerializedProperty serializedDefaultInventory;

        private static bool showEntityTypeInfo;
        private static bool showPlacement;
        private static bool showInventory;
        private static bool showPrefabVariants;
        private static bool showEntityTriggers;
        private static bool showTags;

        private Texture2D expandIcon;
        private Texture2D collapseIcon;
        private Texture2D tagIcon;
        private Texture2D inventoryIcon;
        private Texture2D gearsIcon;
        private Texture2D placementIcon;
        private Texture2D prefabIcon;
        protected Texture2D sectionBackground;
        protected Texture2D personIcon;

        private EntityType entityType;

        public virtual void OnEnable()
        {
            expandIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/expand.png", typeof(Texture2D)) as Texture2D;
            collapseIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/collapse.png", typeof(Texture2D)) as Texture2D;
            personIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/person.png", typeof(Texture2D)) as Texture2D;
            tagIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/tag.png", typeof(Texture2D)) as Texture2D;
            inventoryIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/inventory.png", typeof(Texture2D)) as Texture2D;
            gearsIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/gears.png", typeof(Texture2D)) as Texture2D;
            placementIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/main.png", typeof(Texture2D)) as Texture2D;
            prefabIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/people.png", typeof(Texture2D)) as Texture2D;
            sectionBackground = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/greybackground.png", typeof(Texture2D)) as Texture2D;

            entityType = (EntityType)target;

            serializedTypeCategories = serializedObject.FindProperty("typeCategories");
            typeCategoriesList = new ReorderableList(serializedObject, serializedTypeCategories, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawTypeCategoryListItems,
                onAddCallback = OnAddTypeCategory,
                drawHeaderCallback = DrawTypeCategoryHeader
            };

            serializedPrefabVariants = serializedObject.FindProperty("prefabVariants");
            prefabVariantsList = new ReorderableList(serializedObject, serializedPrefabVariants, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawPrefabVariantListItems,
                onAddCallback = OnAddPrefabVariant,
                drawHeaderCallback = DrawPrefabVariantHeader
            };

            serializedEntityTriggers = serializedObject.FindProperty("defaultEntityTriggers");
            entityTriggersList = new ReorderableList(serializedObject, serializedEntityTriggers, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawEntityTriggerListItems,
                onAddCallback = OnAddEntityTrigger,
                drawHeaderCallback = DrawEntityTriggersHeader
            };

            serializedDefaultTags = serializedObject.FindProperty("defaultTags");
            defaultTagsList = new ReorderableList(serializedObject, serializedDefaultTags, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawDefaultTagListItems,
                drawHeaderCallback = DrawDefaultTagHeader,
                onAddCallback = OnAddDefaultTag
            };

            serializedCanBeInInventorySlots = serializedObject.FindProperty("canBeInInventorySlots");
            canBeInInventorySlotsList = new ReorderableList(serializedObject, serializedCanBeInInventorySlots, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawCanBeInventorySlotListItems,
                drawHeaderCallback = DrawCanBeInventorySlotHeader,
                onAddCallback = OnAddCanBeInventorySlot,
                elementHeightCallback = CanBeInventoryHeight
            };

            serializedInventorySlots = serializedObject.FindProperty("inventorySlots");
            inventorySlotsList = new ReorderableList(serializedObject, serializedInventorySlots, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawInventorySlotListItems,
                drawHeaderCallback = DrawInventorySlotHeader,
                onAddCallback = OnAddInventorySlot,
            };

            serializedDefaultInventory = serializedObject.FindProperty("defaultInventory");
            defaultInventoryList = new ReorderableList(serializedObject, serializedDefaultInventory, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawDefaultInventoryListItems,
                drawHeaderCallback = DrawDefaultInventoryHeader,
                onAddCallback = OnAddDefaultInventory,
                elementHeightCallback = DefaultInventoryHeight
            };

            serializedTransformMappings = serializedObject.FindProperty("transformMappings");
            transformMappingsList = new ReorderableList(serializedObject, serializedTransformMappings, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawTransformMappingsListItems,
                drawHeaderCallback = DrawTransformMappingsHeader,
                onAddCallback = OnAddTransformMapping,
                elementHeightCallback = TransformMappingHeight
            };

            serializedItemAttributeTypeModifiers = serializedObject.FindProperty("itemAttributeTypeModifiers");
            itemAttributeTypeModifiersList = new ReorderableList(serializedObject, serializedItemAttributeTypeModifiers, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawItemAttributeTypeModifiersListItems,
                drawHeaderCallback = DrawItemAttributeTypeModifiersHeader,
                onAddCallback = OnAddItemAttributeTypeModifier,
                elementHeightCallback = ActionItemAttributeTypeModifierHeight
            };

            serializedItemActionSkillModifiers = serializedObject.FindProperty("itemActionSkillModifiers");
            itemActionSkillModifiersList = new ReorderableList(serializedObject, serializedItemActionSkillModifiers, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawItemActionSkillModifiersListItems,
                drawHeaderCallback = DrawItemActionSkillModifiersHeader,
                onAddCallback = OnAddItemActionSkillModifier,
                elementHeightCallback = ActionItemActionSkillModifierHeight
            };

            serializedItemSoundEffects = serializedObject.FindProperty("itemSoundEffects");
            itemSoundEffectsList = new ReorderableList(serializedObject, serializedItemSoundEffects, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawItemSoundEffectsListItems,
                drawHeaderCallback = DrawItemSoundEffectsHeader,
                onAddCallback = OnAddItemSoundEffects,
                elementHeightCallback = ItemSoundEffectsHeight
            };

            serializedItemVisualEffects = serializedObject.FindProperty("itemVisualEffects");
            itemVisualEffectsList = new ReorderableList(serializedObject, serializedItemVisualEffects, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawItemVisualEffectsListItems,
                drawHeaderCallback = DrawItemVisualEffectsHeader,
                onAddCallback = OnAddItemVisualEffects,
                elementHeightCallback = ItemVisualEffectsHeight
            };
        }

        void DrawTransformMappingsHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Transform Mappings", headerStyle);
        }
        
        private void OnAddTransformMapping(ReorderableList list)
        {
            serializedTransformMappings.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("transform").objectReferenceValue = null;
            element.FindPropertyRelative("inventorySlot").objectReferenceValue = null;
            element.FindPropertyRelative("ownerMatchType").enumValueIndex = 0;

            element.FindPropertyRelative("typeGroupMatch").objectReferenceValue = null;
            element.FindPropertyRelative("typeCategoryMatch").objectReferenceValue = null;
            element.FindPropertyRelative("entityTypeMatch").objectReferenceValue = null;

            element.FindPropertyRelative("ownerPrefabVariantIndex").intValue = -1;
            element.FindPropertyRelative("thisPrefabVariantIndex").intValue = -1;
            element.FindPropertyRelative("stateNames").arraySize = 0;
        }

        private float TransformMappingHeight(int index)
        {
            SerializedProperty element = transformMappingsList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty transformMappings = element.FindPropertyRelative("transformMappings");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 8f;
            SerializedProperty stateNames = element.FindPropertyRelative("stateNames");
            if (stateNames.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * stateNames.arraySize;
            }
            return height;
        }

        private void DrawTransformMappingsListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = transformMappingsList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("transform"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("inventorySlot"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("ownerMatchType"));

            currentY += itemHeight;            
            InputCondition.MatchType ownerMatchType = (InputCondition.MatchType)element.FindPropertyRelative("ownerMatchType").enumValueIndex;
            switch (ownerMatchType)
            {
                case InputCondition.MatchType.TypeGroup:
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("typeGroupMatch"));
                    break;
                case InputCondition.MatchType.TypeCategory:
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("typeCategoryMatch"));
                    break;
                case InputCondition.MatchType.EntityType:
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("entityTypeMatch"));
                    break;
            }
            
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("ownerPrefabVariantIndex"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("thisPrefabVariantIndex"));

            currentY += itemHeight;
            SerializedProperty stateNames = element.FindPropertyRelative("stateNames");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), stateNames, new GUIContent("State Name Matches"));

            if (stateNames.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                stateNames.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many State Names?", stateNames.arraySize);
                for (int i = 0; i < stateNames.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty stateName = stateNames.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            stateName, new GUIContent((i + 1) + ". State Name"));
                }
                EditorGUI.indentLevel--;
            }
        }

        void DrawItemAttributeTypeModifiersHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Item Attribute Type Modifiers", headerStyle);
        }

        private void OnAddItemAttributeTypeModifier(ReorderableList list)
        {
            serializedItemAttributeTypeModifiers.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("itemCondition").objectReferenceValue = null;
            element.FindPropertyRelative("modifierType").enumValueIndex = 0;
            element.FindPropertyRelative("attributeType").objectReferenceValue = null;
            element.FindPropertyRelative("modifyValueType").enumValueIndex = 0;
            element.FindPropertyRelative("defaultLevelChange").objectReferenceValue = null;
            element.FindPropertyRelative("defaultMinChange").objectReferenceValue = null;
            element.FindPropertyRelative("defaultMaxChange").objectReferenceValue = null;
            element.FindPropertyRelative("prefabVariantValueMappings").arraySize = 0;
        }

        private float ActionItemAttributeTypeModifierHeight(int index)
        {
            SerializedProperty element = itemAttributeTypeModifiersList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 7f;

            ItemAttributeTypeModifier.ModifierType modifierType =
                (ItemAttributeTypeModifier.ModifierType)element.FindPropertyRelative("modifierType").enumValueIndex;
            if (modifierType == ItemAttributeTypeModifier.ModifierType.Always)
            {
                height += itemHeight * 2;
            }

            SerializedProperty prefabVariantValueMappings = element.FindPropertyRelative("prefabVariantValueMappings");
            if (prefabVariantValueMappings.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * prefabVariantValueMappings.arraySize * 2;
                if (modifierType == ItemAttributeTypeModifier.ModifierType.Always)
                {
                    height += itemHeight * prefabVariantValueMappings.arraySize * 2;
                }
            }
            return height;
        }

        private void DrawItemAttributeTypeModifiersListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = itemAttributeTypeModifiersList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("itemCondition"));

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("modifierType"));
            ItemAttributeTypeModifier.ModifierType modifierType =
                (ItemAttributeTypeModifier.ModifierType)element.FindPropertyRelative("modifierType").enumValueIndex;

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("attributeType"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("modifyValueType"));
            
            if (modifierType == ItemAttributeTypeModifier.ModifierType.Always)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("defaultLevelChange"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("defaultMinChange"));
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("defaultMaxChange"));
            }
            else
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("defaultLevelCurveChange"));
            }

            currentY += itemHeight;
            SerializedProperty prefabVariantValueMappings = element.FindPropertyRelative("prefabVariantValueMappings");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), prefabVariantValueMappings,
                                    new GUIContent("Prefab Variant Value Mappings"));

            if (prefabVariantValueMappings.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                prefabVariantValueMappings.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Mappings?", prefabVariantValueMappings.arraySize);
                for (int i = 0; i < prefabVariantValueMappings.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty prefabVariantValueMapping = prefabVariantValueMappings.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            prefabVariantValueMapping.FindPropertyRelative("prefabVariantIndex"),
                                            new GUIContent((i + 1) + ". Prefab Variant Index"));

                    if (modifierType == ItemAttributeTypeModifier.ModifierType.Always)
                    {
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                prefabVariantValueMapping.FindPropertyRelative("levelChange"),
                                                new GUIContent((i + 1) + ". Level Change"));
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                prefabVariantValueMapping.FindPropertyRelative("minChange"),
                                                new GUIContent((i + 1) + ". Min Change"));
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                prefabVariantValueMapping.FindPropertyRelative("maxChange"),
                                                new GUIContent((i + 1) + ". Max Change"));
                    }
                    else
                    {
                        currentY += itemHeight;
                        EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                prefabVariantValueMapping.FindPropertyRelative("levelCurveChange"),
                                                new GUIContent((i + 1) + ". Level Curve Change"));
                    }
                }
                EditorGUI.indentLevel--;
            }
        }


        void DrawItemActionSkillModifiersHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Item Action Skill Modifiers", headerStyle);
        }

        private void OnAddItemActionSkillModifier(ReorderableList list)
        {
            serializedItemActionSkillModifiers.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("itemCondition").objectReferenceValue = null;
            element.FindPropertyRelative("actionTypes").arraySize = 1;
            element.FindPropertyRelative("modifyValueType").enumValueIndex = 0;
            element.FindPropertyRelative("defaultValueCurve").objectReferenceValue = null;
            element.FindPropertyRelative("prefabVariantValueMappings").arraySize = 0;
        }

        private float ActionItemActionSkillModifierHeight(int index)
        {
            SerializedProperty element = itemActionSkillModifiersList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 6f;

            SerializedProperty actionTypes = element.FindPropertyRelative("actionTypes");
            height += itemHeight * actionTypes.arraySize;
 
            SerializedProperty prefabVariantValueMappings = element.FindPropertyRelative("prefabVariantValueMappings");
            if (prefabVariantValueMappings.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * prefabVariantValueMappings.arraySize * 2;
            }
            return height;
        }

        private void DrawItemActionSkillModifiersListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = itemActionSkillModifiersList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("itemCondition"));

            currentY += itemHeight;
            SerializedProperty actionTypes = element.FindPropertyRelative("actionTypes");
            actionTypes.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                              "How Many Action Types?", actionTypes.arraySize);
            for (int i = 0; i < actionTypes.arraySize; i++)
            {
                currentY += itemHeight;
                EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), actionTypes.GetArrayElementAtIndex(i),
                                        new GUIContent((i + 1) + ". Action Type"));
            }

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("modifyValueType"));
            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("defaultValueCurve"));

            currentY += itemHeight;
            SerializedProperty prefabVariantValueMappings = element.FindPropertyRelative("prefabVariantValueMappings");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), prefabVariantValueMappings,
                                    new GUIContent("Prefab Variant Value Mappings"));

            if (prefabVariantValueMappings.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                prefabVariantValueMappings.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Mappings?", prefabVariantValueMappings.arraySize);
                for (int i = 0; i < prefabVariantValueMappings.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty prefabVariantValueMapping = prefabVariantValueMappings.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            prefabVariantValueMapping.FindPropertyRelative("prefabVariantIndex"),
                                            new GUIContent((i + 1) + ". Prefab Variant Index"));
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            prefabVariantValueMapping.FindPropertyRelative("valueCurve"),
                                            new GUIContent((i + 1) + ". Value Curve"));
                }
                EditorGUI.indentLevel--;
            }
        }

        void DrawItemSoundEffectsHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Item Sound Effects", headerStyle);
        }

        private void OnAddItemSoundEffects(ReorderableList list)
        {
            serializedItemSoundEffects.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("itemCondition").objectReferenceValue = null;
            element.FindPropertyRelative("defaultAudioClipTags").arraySize = 0;
            element.FindPropertyRelative("prefabVariantClipsMappings").arraySize = 0;
        }

        private float ItemSoundEffectsHeight(int index)
        {
            SerializedProperty element = itemSoundEffectsList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty itemSoundEffects = element.FindPropertyRelative("itemSoundEffects");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 4f;

            SerializedProperty defaultAudioClipTags = element.FindPropertyRelative("defaultAudioClipTags");
            if (defaultAudioClipTags.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * defaultAudioClipTags.arraySize;
            }
            SerializedProperty prefabVariantClipsMappings = element.FindPropertyRelative("prefabVariantClipsMappings");
            if (prefabVariantClipsMappings.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * prefabVariantClipsMappings.arraySize * 2;

                for (int i = 0; i < prefabVariantClipsMappings.arraySize; i++)
                {
                    SerializedProperty prefabVariantClipsMapping = prefabVariantClipsMappings.GetArrayElementAtIndex(i);
                    SerializedProperty audioClipTags = prefabVariantClipsMapping.FindPropertyRelative("audioClipTags");
                    if (audioClipTags.isExpanded)
                    {
                        height += itemHeight;
                        height += itemHeight * audioClipTags.arraySize;
                    }
                }
            }
            return height;
        }

        private void DrawItemSoundEffectsListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element =  itemSoundEffectsList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("itemCondition"));

            currentY += itemHeight;
            SerializedProperty defaultAudioClipTags = element.FindPropertyRelative("defaultAudioClipTags");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), defaultAudioClipTags,
                                    new GUIContent("Default Audio Clips"));

            if (defaultAudioClipTags.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                defaultAudioClipTags.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Audio Clips?", defaultAudioClipTags.arraySize);
                for (int i = 0; i < defaultAudioClipTags.arraySize; i++)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth - 80f, lineHeight),
                                            defaultAudioClipTags.GetArrayElementAtIndex(i).FindPropertyRelative("audioClip"),
                                            new GUIContent((i + 1) + ". Audio Clip"));
                    EditorGUI.PropertyField(new Rect(rect.x + itemWidth - 90f, currentY, 90f, lineHeight),
                                            defaultAudioClipTags.GetArrayElementAtIndex(i).FindPropertyRelative("tag"),
                                            GUIContent.none);
                }
                EditorGUI.indentLevel--;
            }

            currentY += itemHeight;
            SerializedProperty prefabVariantClipsMappings = element.FindPropertyRelative("prefabVariantClipsMappings");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), prefabVariantClipsMappings,
                                    new GUIContent("Prefab Variant Audio Clips Mappings"));

            if (prefabVariantClipsMappings.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                prefabVariantClipsMappings.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Mappings?", prefabVariantClipsMappings.arraySize);
                for (int i = 0; i < prefabVariantClipsMappings.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty prefabVariantClipsMapping = prefabVariantClipsMappings.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            prefabVariantClipsMapping.FindPropertyRelative("prefabVariantIndex"),
                                            new GUIContent((i + 1) + ". Prefab Variant Index"));

                    currentY += itemHeight;
                    SerializedProperty audioClipTags = prefabVariantClipsMapping.FindPropertyRelative("audioClipTags");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), audioClipTags,
                                            new GUIContent((i + 1) + ". Audio Clips"));

                    if (audioClipTags.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        currentY += itemHeight;
                        audioClipTags.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                         "How Many Audio Clips?", audioClipTags.arraySize);
                        for (int j = 0; j < audioClipTags.arraySize; j++)
                        {
                            currentY += itemHeight;
                            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth - 80f, lineHeight),
                                                    audioClipTags.GetArrayElementAtIndex(j).FindPropertyRelative("audioClip"),
                                                    new GUIContent((j + 1) + ". Audio Clip"));
                            EditorGUI.PropertyField(new Rect(rect.x + itemWidth - 105f, currentY, 105f, lineHeight),
                                                    audioClipTags.GetArrayElementAtIndex(j).FindPropertyRelative("tag"),
                                                    GUIContent.none);
                        }
                        EditorGUI.indentLevel--;
                    }

                }
                EditorGUI.indentLevel--;
            }
        }


        void DrawItemVisualEffectsHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Item Visual Effects", headerStyle);
        }

        private void OnAddItemVisualEffects(ReorderableList list)
        {
            serializedItemVisualEffects.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("itemCondition").objectReferenceValue = null;
            element.FindPropertyRelative("defaultGameObjectTags").arraySize = 0;
            element.FindPropertyRelative("prefabVariantGameObjectsMappings").arraySize = 0;
        }

        private float ItemVisualEffectsHeight(int index)
        {
            SerializedProperty element = itemVisualEffectsList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty itemSoundEffects = element.FindPropertyRelative("itemVisualEffects");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 4f;

            SerializedProperty defaultGameObjects = element.FindPropertyRelative("defaultGameObjectTags");
            if (defaultGameObjects.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * defaultGameObjects.arraySize;
            }
            SerializedProperty prefabVariantGameObjectsMappings = element.FindPropertyRelative("prefabVariantGameObjectsMappings");
            if (prefabVariantGameObjectsMappings.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * prefabVariantGameObjectsMappings.arraySize * 2;

                for (int i = 0; i < prefabVariantGameObjectsMappings.arraySize; i++)
                {
                    SerializedProperty prefabVariantGameObjectsMapping = prefabVariantGameObjectsMappings.GetArrayElementAtIndex(i);
                    SerializedProperty gameObjectTags = prefabVariantGameObjectsMapping.FindPropertyRelative("gameObjectTags");
                    if (gameObjectTags.isExpanded)
                    {
                        height += itemHeight;
                        height += itemHeight * gameObjectTags.arraySize;
                    }
                }
            }
            return height;
        }

        private void DrawItemVisualEffectsListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = itemVisualEffectsList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), element.FindPropertyRelative("itemCondition"));

            currentY += itemHeight;
            SerializedProperty defaultGameObjectTags = element.FindPropertyRelative("defaultGameObjectTags");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), defaultGameObjectTags,
                                    new GUIContent("Default Game Objects"));

            if (defaultGameObjectTags.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                defaultGameObjectTags.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Game Objects?", defaultGameObjectTags.arraySize);
                for (int i = 0; i < defaultGameObjectTags.arraySize; i++)
                {
                    currentY += itemHeight;
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth - 80f, lineHeight),
                                            defaultGameObjectTags.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject"),
                                            new GUIContent((i + 1) + ". Game Object"));
                    EditorGUI.PropertyField(new Rect(rect.x + itemWidth - 90f, currentY, 90f, lineHeight),
                                            defaultGameObjectTags.GetArrayElementAtIndex(i).FindPropertyRelative("tag"),
                                            GUIContent.none);
                }
                EditorGUI.indentLevel--;
            }

            currentY += itemHeight;
            SerializedProperty prefabVariantGameObjectsMappings = element.FindPropertyRelative("prefabVariantGameObjectsMappings");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), prefabVariantGameObjectsMappings,
                                    new GUIContent("Prefab Variant Audio Clips Mappings"));

            if (prefabVariantGameObjectsMappings.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                prefabVariantGameObjectsMappings.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                 "How Many Mappings?", prefabVariantGameObjectsMappings.arraySize);
                for (int i = 0; i < prefabVariantGameObjectsMappings.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty prefabVariantGameObjectsMapping = prefabVariantGameObjectsMappings.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            prefabVariantGameObjectsMapping.FindPropertyRelative("prefabVariantIndex"),
                                            new GUIContent((i + 1) + ". Prefab Variant Index"));

                    currentY += itemHeight;
                    SerializedProperty gameObjectTags = prefabVariantGameObjectsMapping.FindPropertyRelative("gameObjectTags");
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), gameObjectTags,
                                            new GUIContent((i + 1) + ". Game Objects"));

                    if (gameObjectTags.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        currentY += itemHeight;
                        gameObjectTags.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                         "How Many Game Objects?", gameObjectTags.arraySize);
                        for (int j = 0; j < gameObjectTags.arraySize; j++)
                        {
                            currentY += itemHeight;
                            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth - 80f, lineHeight),
                                                    gameObjectTags.GetArrayElementAtIndex(j).FindPropertyRelative("gameObject"),
                                                    new GUIContent((j + 1) + ". Game Object"));
                            EditorGUI.PropertyField(new Rect(rect.x + itemWidth - 105f, currentY, 105f, lineHeight),
                                                    gameObjectTags.GetArrayElementAtIndex(j).FindPropertyRelative("tag"),
                                                    GUIContent.none);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawEntityTriggersHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 1, 300, EditorGUIUtility.singleLineHeight + 5), "Entity Triggers", headerStyle);
        }
        private void OnAddEntityTrigger(ReorderableList list)
        {
            serializedEntityTriggers.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawEntityTriggerListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = entityTriggersList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        private void DrawTypeCategoryHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 1, 300, EditorGUIUtility.singleLineHeight + 5), "Type Categories", headerStyle);
        }
        private void OnAddTypeCategory(ReorderableList list)
        {
            serializedTypeCategories.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawTypeCategoryListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = typeCategoriesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        /*
        private void OnAddTypeCategory(ReorderableList list)
        {
            serializedDefaultTags.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("type").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
        }
        */
        private void DrawPrefabVariantHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 1, 300, EditorGUIUtility.singleLineHeight + 5), "Possible Prefabs For This Type", headerStyle);
        }
        private void OnAddPrefabVariant(ReorderableList list)
        {
            serializedPrefabVariants.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawPrefabVariantListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = prefabVariantsList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        private void OnAddDefaultTag(ReorderableList list)
        {
            serializedDefaultTags.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("type").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
        }

        private void DrawDefaultTagListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = defaultTagsList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 250, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("type"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 275, rect.y, 60, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("level"), GUIContent.none);
        }

        private void DrawDefaultTagHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Tag Type", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 290, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Level", headerStyle);
        }

        private float CanBeInventoryHeight(int index)
        {
            SerializedProperty element = canBeInInventorySlotsList.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty canBeInInventorySlots = element.FindPropertyRelative("canBeInInventorySlots");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 3f;
            SerializedProperty otherInventorySlots = element.FindPropertyRelative("otherInventorySlots");
            if (otherInventorySlots.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * element.FindPropertyRelative("otherInventorySlots").arraySize * 2;
            }
            return height;
        }

        private void OnAddCanBeInventorySlot(ReorderableList list)
        {
            serializedCanBeInInventorySlots.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("inventorySlot").objectReferenceValue = null;
            element.FindPropertyRelative("numLocations").intValue = 1;
            element.FindPropertyRelative("isEquipped").boolValue = false;
            element.FindPropertyRelative("otherInventorySlots").arraySize = 0;
            element.FindPropertyRelative("otherInventorySlotsNumLocations").arraySize = 0;
        }

        private void DrawCanBeInventorySlotListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = canBeInInventorySlotsList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, 240, EditorGUIUtility.singleLineHeight + 2),
                                    element.FindPropertyRelative("inventorySlot"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 270, currentY, 30, EditorGUIUtility.singleLineHeight + 2),
                                    element.FindPropertyRelative("isEquipped"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + 320, currentY, 30, EditorGUIUtility.singleLineHeight + 2),
                                    element.FindPropertyRelative("numLocations"), GUIContent.none);

            currentY += itemHeight;
            SerializedProperty otherInventorySlots = element.FindPropertyRelative("otherInventorySlots");
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight), otherInventorySlots, new GUIContent("Multi-Slot"));

            if (otherInventorySlots.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                otherInventorySlots.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                             "How Many Other Slots?", otherInventorySlots.arraySize);
                element.FindPropertyRelative("otherInventorySlotsNumLocations").arraySize = otherInventorySlots.arraySize;
                for (int i = 0; i < otherInventorySlots.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty otherInventorySlot = otherInventorySlots.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            otherInventorySlot, new GUIContent((i + 1) + ". Other Slot"));
                    currentY += itemHeight;
                    SerializedProperty otherInventorySlotsNumLocation = element.FindPropertyRelative("otherInventorySlotsNumLocations")
                                                                               .GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            otherInventorySlotsNumLocation, new GUIContent((i + 1) + ". Number Locations"));
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawCanBeInventorySlotHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Can Be In Inventory Slots", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 250, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Is Equipped?", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 325, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "# Locations", headerStyle);
        }

        private void OnAddInventorySlot(ReorderableList list)
        {
            serializedInventorySlots.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawInventorySlotListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = inventorySlotsList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 350, EditorGUIUtility.singleLineHeight + 2), element, GUIContent.none);
        }
        
        private void DrawInventorySlotHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Inventory Slots", headerStyle);
        }

        private void OnAddDefaultInventory(ReorderableList list)
        {
            serializedDefaultInventory.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("inventorySlot").objectReferenceValue = null;
            element.FindPropertyRelative("entityType").objectReferenceValue = null;
            element.FindPropertyRelative("amountCurve").objectReferenceValue = null;
            element.FindPropertyRelative("probability").floatValue = 100;
        }

        private float DefaultInventoryHeight(int index)
        {
            SerializedProperty element = defaultInventoryList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemHeight = lineHeight + 3f;
            float height = itemHeight * 4f;
            return height;
        }

        void DrawDefaultInventoryListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = defaultInventoryList.serializedProperty.GetArrayElementAtIndex(index);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f, lineHeight),
                                    element.FindPropertyRelative("inventorySlot"), GUIContent.none);

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f, lineHeight),
                                    element.FindPropertyRelative("entityType"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 10f, currentY, 35, lineHeight),
                                    element.FindPropertyRelative("prefabVariantIndex"), GUIContent.none);
            EditorGUI.LabelField(new Rect(rect.x + itemWidth / 2f + 50f, currentY, 120f, lineHeight), "Prefab Variant Index");

            currentY += itemHeight;
            EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth / 2f, lineHeight),
                                    element.FindPropertyRelative("amountCurve"), GUIContent.none);
            EditorGUI.PropertyField(new Rect(rect.x + itemWidth / 2f + 10f, currentY, 35, lineHeight),
                                    element.FindPropertyRelative("probability"), GUIContent.none);
            EditorGUI.LabelField(new Rect(rect.x + itemWidth / 2f + 50f, currentY, 75f, lineHeight), "Probability");
        }

        void DrawDefaultInventoryHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x + 2, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Default Inventory", headerStyle);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            headerStyle = new GUIStyle("label")
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12
            };

            Color sectionColor = EditorUtilities.L2;
            

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool expand = GUILayout.Button(expandIcon, new GUIStyle(EditorStyles.toolbarButton)
                              { margin = new RectOffset(3, 3, 8, 0) }, GUILayout.Width(24f));
            bool collapse = GUILayout.Button(collapseIcon, new GUIStyle(EditorStyles.toolbarButton)
                                { margin = new RectOffset(3, 8, 8, 0) }, GUILayout.Width(24f));
            GUILayout.EndHorizontal();

            if (expand)
                ExpandAll(true);
            if (collapse)
                ExpandAll(false);

            int numTypeCategories = serializedObject.FindProperty("typeCategories").arraySize;
            string rightInfo = "";
            for (int i = 0; i < numTypeCategories; i++)
            {
                if (serializedObject.FindProperty("typeCategories").GetArrayElementAtIndex(i).objectReferenceValue != null)
                    rightInfo += serializedObject.FindProperty("typeCategories").GetArrayElementAtIndex(i).objectReferenceValue.name + ",";
            }
            rightInfo = rightInfo.Length == 0 ? "None" : rightInfo.Substring(0, rightInfo.Length -1);
            rightInfo += " (" + serializedObject.FindProperty("maxDistanceAsInput").floatValue + ")";
            showEntityTypeInfo = EditorUtilities.BeginInspectorSection("Entity Type Info", personIcon, showEntityTypeInfo, sectionColor,
                                                                       sectionBackground, rightInfo);
            if (showEntityTypeInfo)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistanceAsInput"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionSpotType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("spotTypeActionTypeMappings"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sideEffectValue"));
                GUILayout.Space(10f);
                typeCategoriesList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showEntityTypeInfo);

            int numPrefabVariants = serializedObject.FindProperty("prefabVariants").arraySize;
            rightInfo = numPrefabVariants > 0 ? numPrefabVariants.ToString() : "None";
            showPrefabVariants = EditorUtilities.BeginInspectorSection("Prefab Variants", prefabIcon, showPrefabVariants, sectionColor,
                                                                       sectionBackground, rightInfo);
            if (showPrefabVariants)
            {
                prefabVariantsList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showPrefabVariants);

            showPlacement = EditorUtilities.BeginInspectorSection("Placement Info", placementIcon, showPlacement, sectionColor, sectionBackground);
            if (showPlacement)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ghostPrefabs"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("requiresFlatLand"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotateToTerrain"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSlopePlacement"));
            }
            EditorUtilities.EndInspectorSection(showPlacement);

            int numTags = serializedObject.FindProperty("defaultTags").arraySize;
            rightInfo = numTags > 0 ? numTags.ToString() : "None";
            showTags = EditorUtilities.BeginInspectorSection("Tags", tagIcon, showTags, sectionColor, sectionBackground, rightInfo);
            if (showTags)
            {
                defaultTagsList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showTags);

            if (serializedObject.FindProperty("defaultInventoryType").objectReferenceValue == null)
                showInventory = true;

            int numSlots = serializedObject.FindProperty("inventorySlots").arraySize;
            int numCanBeInSlots = serializedObject.FindProperty("canBeInInventorySlots").arraySize;
            rightInfo = numSlots > 0 ? numSlots.ToString() + " Slot" + (numSlots > 1 ? "s" : "") + ", " : "Can't Have, ";
            rightInfo += numCanBeInSlots > 0 ? numCanBeInSlots.ToString() + " Slot" + (numCanBeInSlots > 1 ? "s" : "") +
                         " Can Be In" : "Can't Be";
            showInventory = EditorUtilities.BeginInspectorSection("Inventory", inventoryIcon, showInventory, sectionColor,
                                                                  sectionBackground, rightInfo);
            if (showInventory)
            {
                if (serializedObject.FindProperty("defaultInventoryType").objectReferenceValue == null)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox("InventoryType must be specified.", MessageType.Error);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultInventoryType"));
                GUILayout.Space(15);

                canBeInInventorySlotsList.DoLayoutList();
                GUILayout.Space(15);

                if (entityType.canBeInInventorySlots != null && entityType.canBeInInventorySlots.Count > 0 && entityType.canBeInInventorySlots[0] != null)
                {
                    transformMappingsList.DoLayoutList();
                    GUILayout.Space(15);

                    itemAttributeTypeModifiersList.DoLayoutList();
                    GUILayout.Space(15);

                    itemActionSkillModifiersList.DoLayoutList();
                    GUILayout.Space(15);

                    itemSoundEffectsList.DoLayoutList();
                    GUILayout.Space(15);

                    itemVisualEffectsList.DoLayoutList();
                    GUILayout.Space(15);
                }

                inventorySlotsList.DoLayoutList();

                if (entityType.inventorySlots != null && entityType.inventorySlots.Count > 0 && entityType.inventorySlots[0] != null)
                {
                    if (entityType.defaultInventory != null)
                    {
                        // Make sure inventory slot exists and that the EntityType is allowed in that slot
                        // TODO: Turn into dropdown with inventory slots as options

                        foreach (EntityType.DefaultInventory defaultInventory in entityType.defaultInventory)
                        {
                            if (defaultInventory.inventorySlot != null && !entityType.inventorySlots.Contains(defaultInventory.inventorySlot))
                            {
                                GUILayout.Space(15);
                                EditorGUILayout.HelpBox("Default Inventory is using an InventorySlot (" + defaultInventory.inventorySlot.name +
                                                        ") that the EntityType does not have.", MessageType.Error);
                            }

                            if (defaultInventory.inventorySlot != null && defaultInventory.entityType != null &&
                                !defaultInventory.inventorySlot.EntityTypeAllowed(defaultInventory.entityType))
                            {
                                GUILayout.Space(15);
                                EditorGUILayout.HelpBox("Default Inventory has an EntityType (" + defaultInventory.entityType.name + ") in an InventorySlot (" +
                                                        defaultInventory.inventorySlot.name + ") that is is not allowed to be in.  Please check the " +
                                                        "EntityType's Categories and the InventorySlot's Allowed Categories.", MessageType.Error);
                            }
                        }
                    }

                    GUILayout.Space(15);
                    defaultInventoryList.DoLayoutList();

                    WorldObjectType worldObjectType = entityType as WorldObjectType;
                    if (worldObjectType != null && worldObjectType.defaultInventory != null && worldObjectType.defaultInventory.Count > 0)
                    {
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("addDefaultInventoryTiming"));
                        GUILayout.Space(5);
                    }
                }
            }
            EditorUtilities.EndInspectorSection(showInventory);

            int numEntityTriggers = serializedObject.FindProperty("defaultEntityTriggers").arraySize;
            rightInfo = numEntityTriggers > 0 ? numEntityTriggers.ToString() : "None";
            showEntityTriggers = EditorUtilities.BeginInspectorSection("Entity Triggers", gearsIcon, showEntityTriggers, sectionColor,
                                                                        sectionBackground, rightInfo);
            if (showEntityTriggers)
            {
                entityTriggersList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showEntityTriggers);
            
        }

        private void ExpandAll(bool expand)
        {
            showEntityTypeInfo = expand;
            showPlacement = expand;
            showInventory = expand;
            showPrefabVariants = expand;
            showEntityTriggers = expand;
            showTags = expand;
            showEntityTriggers = expand;
            AgentTypeEditor.showCoreTypes = expand;
            AgentTypeEditor.showDriveTypes = expand;
            AgentTypeEditor.showActionTypes = expand;
            AgentTypeEditor.showAttributeTypes = expand;
            AgentTypeEditor.showAnimatorOverrides = expand;
            WorldObjectTypeEditor.showcompleteInfo = expand;
            WorldObjectTypeEditor.showStates = expand;
            WorldObjectTypeEditor.showdamageInfo = expand;
            WorldObjectTypeEditor.showSkinPrefabs = expand;
            WorldObjectTypeEditor.showRecipes = expand;
        }
    }
}
