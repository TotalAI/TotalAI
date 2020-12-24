using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using System.Linq;

namespace TotalAI.Editor
{
    public class EntityEditor : UnityEditor.Editor
    {
        public GUIStyle headerStyle;

        private ReorderableList defaultTagsList;
        private SerializedProperty serializedDefaultTags;

        private ReorderableList overridesList;
        private SerializedProperty serializedOverrides;

        private ReorderableList inventorySlotMappingsList;
        private SerializedProperty serializedInventorySlotMappings;

        private float lineHeight;
        protected Entity entity;

        private static bool showTags;
        private static bool showOverrides;
        private static bool showInventory;

        private Texture2D tagIcon;
        private Texture2D inventoryIcon;
        private Texture2D overrideIcon;
        protected Texture2D sectionBackground;

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public virtual void OnEnable()
        {
            tagIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/tag.png", typeof(Texture2D)) as Texture2D;
            inventoryIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/inventory.png", typeof(Texture2D)) as Texture2D;
            overrideIcon = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/override.png", typeof(Texture2D)) as Texture2D;
            sectionBackground = AssetDatabase.LoadAssetAtPath("Assets/TotalAI/Editor/Images/greybackground.png", typeof(Texture2D)) as Texture2D;

            entity = (Entity)target;
            lineHeight = EditorGUIUtility.singleLineHeight;

            
            serializedDefaultTags = serializedObject.FindProperty("defaultTagsWithEntity");
            defaultTagsList = new ReorderableList(serializedObject, serializedDefaultTags, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawDefaultTagListItems,
                drawHeaderCallback = DrawDefaultTagHeader,
                onAddCallback = OnAddDefaultTag,
            };

            serializedOverrides = serializedObject.FindProperty("entityTypeOverrides");
            overridesList = new ReorderableList(serializedObject, serializedOverrides, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawOverrideItems,
                onAddCallback = OnAddOverride,
                headerHeight = 1
            };

            serializedInventorySlotMappings = serializedObject.FindProperty("inventorySlotMappings");
            inventorySlotMappingsList = new ReorderableList(serializedObject, serializedInventorySlotMappings, true, true, true, true)
            {
                drawElementBackgroundCallback = EditorUtilities.DrawReordableListBackground,
                drawElementCallback = DrawInventorySlotMappingsItems,
                onAddCallback = OnAddInventorySlotMappings,
                elementHeightCallback = InventorySlotMappingsHeight,
                headerHeight = 1
            };
        }

        private void OnAddDefaultTag(ReorderableList list)
        {
            serializedDefaultTags.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("entity").objectReferenceValue = null;
            element.FindPropertyRelative("type").objectReferenceValue = null;
            element.FindPropertyRelative("level").floatValue = 0;
        }

        void DrawDefaultTagListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = defaultTagsList.serializedProperty.GetArrayElementAtIndex(index);

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("entity"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 205, rect.y, 200, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("type"), GUIContent.none);

            EditorGUI.PropertyField(new Rect(rect.x + 415, rect.y, 40, EditorGUIUtility.singleLineHeight),
                                    element.FindPropertyRelative("level"), GUIContent.none);
        }

        void DrawDefaultTagHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 125, EditorGUIUtility.singleLineHeight + 5), "Entity", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 210, rect.y + 1, 125, EditorGUIUtility.singleLineHeight + 5), "Default Tag", headerStyle);
            EditorGUI.LabelField(new Rect(rect.x + 415, rect.y + 1, 100, EditorGUIUtility.singleLineHeight + 5), "Level", headerStyle);
        }

        private void OnAddOverride(ReorderableList list)
        {
            serializedOverrides.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.objectReferenceValue = null;
        }

        private void DrawOverrideItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = overridesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 300, EditorGUIUtility.singleLineHeight + 2), element, GUIContent.none);
        }

        private void DrawOverrideHeader(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, EditorGUIUtility.singleLineHeight + 5), "Entity Type Overrides", headerStyle);
        }


        private void OnAddInventorySlotMappings(ReorderableList list)
        {
            serializedInventorySlotMappings.arraySize++;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            element.FindPropertyRelative("inventorySlot").objectReferenceValue = null;
            element.FindPropertyRelative("locations").ClearArray();
        }

        private float InventorySlotMappingsHeight(int index)
        {
            SerializedProperty element = inventorySlotMappingsList.serializedProperty.GetArrayElementAtIndex(index);
            
            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;

            float height = itemHeight * 2f;


            SerializedProperty locations = element.FindPropertyRelative("locations");
            if (locations.isExpanded)
            {
                height += itemHeight;
                height += itemHeight * element.FindPropertyRelative("locations").arraySize;
            }

            return height + 20f;
        }

        void DrawInventorySlotMappingsItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = inventorySlotMappingsList.serializedProperty.GetArrayElementAtIndex(index);

            float itemWidth = EditorGUIUtility.currentViewWidth - 75f;
            float itemHeight = lineHeight + 3f;
            float currentY = rect.y + 10f;

            EditorGUI.PropertyField(new Rect(rect.x, currentY, 300, lineHeight),
                                    element.FindPropertyRelative("inventorySlot"), GUIContent.none);


            currentY += itemHeight;
            SerializedProperty locations = element.FindPropertyRelative("locations");
            EditorGUI.PropertyField(new Rect(rect.x + 13, currentY, itemWidth, lineHeight), locations);

            if (locations.isExpanded)
            {
                EditorGUI.indentLevel++;
                currentY += itemHeight;
                locations.arraySize = EditorGUI.DelayedIntField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                                                "How Many Locations?", locations.arraySize);

                for (int i = 0; i < locations.arraySize; i++)
                {
                    currentY += itemHeight;
                    SerializedProperty location = locations.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(rect.x, currentY, itemWidth, lineHeight),
                                            location, new GUIContent("Location #" + (i + 1)));
                }
                EditorGUI.indentLevel--;
            }
        }

        void DrawInventorySlotMappingsHeader(Rect rect)
        {
            rect = new Rect();
            //EditorGUI.LabelField(new Rect(rect.x, rect.y + 1, 200, 1), "Inventory Slots Mappings", headerStyle);
        }

        public override void OnInspectorGUI()
        {
            string rightInfo;
            headerStyle = new GUIStyle("label")
            {
                fontSize = 12,
                alignment = TextAnchor.UpperLeft
            };

            Color sectionColor = EditorUtilities.L2;

            Type type = EntityType.GetEntityType(entity);

            rightInfo = entity.defaultTagsWithEntity == null || entity.defaultTagsWithEntity.Count == 0 ?
                        "None" : entity.defaultTagsWithEntity.Count.ToString();
            showTags = EditorUtilities.BeginInspectorSection("Tags", tagIcon, showTags, sectionColor, sectionBackground, rightInfo);
            if (showTags)
            {
                defaultTagsList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showTags);

            rightInfo = entity.entityTypeOverrides == null || entity.entityTypeOverrides.Count == 0 ?
                        "None" : string.Join(", ", entity.entityTypeOverrides.Select(x => x == null ? "Null" : x.name));
            showOverrides = EditorUtilities.BeginInspectorSection("Overrides", overrideIcon, showOverrides, sectionColor,
                                                                  sectionBackground, rightInfo);
            if (showOverrides)
            {
                overridesList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showOverrides);

            // Show error if not all required InventorySlots have Mappings
            bool missingMappings = !entity.HasRequiredInventorySlotMappings();
            if (missingMappings)
                showInventory = true;

            rightInfo = entity.inventorySlotMappings == null || entity.inventorySlotMappings.Count == 0 ?
                        "None" : entity.inventorySlotMappings.Count.ToString();
            showInventory = EditorUtilities.BeginInspectorSection("Inventory Slot Mappings", inventoryIcon, showInventory,
                                                                  sectionColor, sectionBackground, rightInfo);
            if (showInventory)
            {
                if (missingMappings)
                {
                    GUILayout.Space(3);
                    EditorGUILayout.HelpBox("Inventory Slot Mappings don't match with the Inventory Slots.", MessageType.Error);
                    GUILayout.Space(5);
                }
                inventorySlotMappingsList.DoLayoutList();
            }
            EditorUtilities.EndInspectorSection(showInventory);
        }
    }
}
