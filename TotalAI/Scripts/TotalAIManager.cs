using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    // TODO: Remove this from build
    [InitializeOnLoad]
    public static class UpdateTypesBeforePlay
    {
        static UpdateTypesBeforePlay()
        {
            EditorApplication.playModeStateChanged += UpdateManagerTypeLists;
        }

        private static void UpdateManagerTypeLists(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                TotalAIManager manager = Object.FindObjectOfType<TotalAIManager>();
                if (manager == null)
                {
                    Debug.LogError("No TotalAIManager in the scene - Please Fix.");
                    return;
                }

                TotalAIManager.CreateTypeLists(manager);

                TypeCategory.UpdateTypeCategoriesChildren();
            }
        }
    }

    public class TotalAIManager : MonoBehaviour
    {
        // Leave null if not using
        private GUIManager guiManager;

        public TotalAISettings settings;

        // A Complete dictionary mapping all the matches for an IC to a MT (MT has at least one OC that fixes the IC)
        public class FixInputCondition
        {
            public MappingType mappingType;
            public int outputChangeIndex;
        }
        public Dictionary<InputCondition, List<FixInputCondition>> fixesInputCondition;

        // Fake MappingTypes for DriveLevels with just one InputCondition that will map to reducing DriveLevel OCs
        public Dictionary<DriveType, MappingType> fakeDriveLevelMappingTypes;

        // List of various SOs being used by the project - needed for runtime
        public List<TypeCategory> allTypeCategories;
        public List<InputOutputType> allInputOutputTypes;
        public List<EntityType> allEntityTypes;
        public List<TypeGroup> allTypeGroups;

        // Grabs these from settings when running - provides a way to update all NavMeshes in Scene
        private List<MovementType> movementTypes;
        
        public Dictionary<TypeCategory, List<InputOutputType>> typeCategoryToIOTs;

        // Entity Loopup Dictionary
        public Dictionary<EntityType, List<Entity>> entityTypeToEntityInScene = new Dictionary<EntityType, List<Entity>>();

        public static TotalAIManager manager;

        private void Start()
        {
            if (manager != null)
            {
                Debug.LogError("Multiple TotalAIManagers in the scene - Please Fix.");
                return;
            }
            manager = this;

            if (manager.settings == null)
            {
                Debug.LogError("TotalAIManager has no TotalAISettings set - Please Fix.");
                return;
            }

            guiManager = FindObjectOfType<GUIManager>();
            
            // Create the IC to MT/OC Dict for quick Matching when planning
            CreateICToMTDictionary();

            CreateTypeCategoryToIOTsDictionary();

            // Update NavMesh
            CreateMovementTypeList();
            UpdateAllNavMeshes();
        }

        // Called by Entity Disabled Event - remove this entity
        private void EntityDestroyed(Entity entity)
        {
            entity.EntityDestroyedEvent -= EntityDestroyed;
            entityTypeToEntityInScene[entity.entityType].Remove(entity);

            // If Agent and using GUIManager - let GUIManager know about agent being disabled
            if (entity is Agent agent && guiManager != null)
            {
                guiManager.DisableAgent(agent);
            }
        }

        public void EntityEnabled(Entity entity)
        {
            entity.EntityDestroyedEvent += EntityDestroyed;
            if (entity.entityType == null)
            {
                Debug.LogError(entity.name + " is missing an EntityType.  Please fix in the inspector.");
                return;
            }
            if (entityTypeToEntityInScene.TryGetValue(entity.entityType, out List<Entity> currentEntities))
            {
                currentEntities.Add(entity);
            }
            else
            {
                entityTypeToEntityInScene[entity.entityType] = new List<Entity>() { entity };
            }

            // If Agent and using GUIManager - let GUIManager know about new Agent
            if (entity is Agent agent && guiManager != null)
            {
                guiManager.EnableAgent(agent);
            }
        }

        // Called before play to keep these lists up to date
        public static void CreateTypeLists(TotalAIManager manager)
        {
            List<TypeCategory> allTCs = new List<TypeCategory>();
            var guids = AssetDatabase.FindAssets("t:TypeCategory");
            foreach (string guid in guids)
            {
                allTCs.Add((TypeCategory)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            manager.allTypeCategories = new List<TypeCategory>();
            foreach (TypeCategory typeCategory in allTCs)
            {
                manager.allTypeCategories.Add(typeCategory);
            }

            List<InputOutputType> allIOTs = new List<InputOutputType>();
            guids = AssetDatabase.FindAssets("t:InputOutputType");
            foreach (string guid in guids)
            {
                allIOTs.Add((InputOutputType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            manager.allInputOutputTypes = new List<InputOutputType>();
            manager.allEntityTypes = new List<EntityType>();
            foreach (InputOutputType inputOutputType in allIOTs)
            {
                manager.allInputOutputTypes.Add(inputOutputType);
                if (inputOutputType is EntityType)
                    manager.allEntityTypes.Add((EntityType)inputOutputType);
            }

            List<TypeGroup> allTypeGroups = new List<TypeGroup>();
            guids = AssetDatabase.FindAssets("t:TypeGroup");
            foreach (string guid in guids)
            {
                allTypeGroups.Add((TypeGroup)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            manager.allTypeGroups = new List<TypeGroup>();
            foreach (TypeGroup typeGroup in allTypeGroups)
            {
                manager.allTypeGroups.Add(typeGroup);
            }

            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateTypeCategoryToIOTsDictionary()
        {
            typeCategoryToIOTs = new Dictionary<TypeCategory, List<InputOutputType>>();

            foreach (InputOutputType inputOutputType in allInputOutputTypes)
            {
                foreach (TypeCategory typeCategory in inputOutputType.typeCategories)
                {
                    if (typeCategory == null)
                    {
                        Debug.LogError(inputOutputType.name + " has a null TypeCategory.  Please Fix. (In the InputOutputType section)");
                    }
                    if (typeCategoryToIOTs.TryGetValue(typeCategory, out List<InputOutputType> inputOutputTypes))
                    {
                        inputOutputTypes.Add(inputOutputType);
                    }
                    else
                    {
                        typeCategoryToIOTs[typeCategory] = new List<InputOutputType>() { inputOutputType };
                    }
                }
            }

            // Go through list of all TypeCategories and see if any where not found in at least one IOT
            foreach (TypeCategory typeCategory in allTypeCategories)
            {
                if (!typeCategoryToIOTs.ContainsKey(typeCategory))
                {
                    typeCategoryToIOTs[typeCategory] = new List<InputOutputType>() { };
                }
            }
        }

        public void CreateICToMTDictionary()
        {
            if (settings.driveLevelICT == null)
            {
                return;
            }

            fixesInputCondition = new Dictionary<InputCondition, List<FixInputCondition>>();
            fakeDriveLevelMappingTypes = new Dictionary<DriveType, MappingType>();

            // TODO: Will this work in runtime?
            List<MappingType> allMappingTypes = new List<MappingType>();
            string[] guids = AssetDatabase.FindAssets("t:MappingType");
            foreach (string guid in guids)
            {
                allMappingTypes.Add((MappingType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            List<DriveType> allDriveTypes = new List<DriveType>();
            guids = AssetDatabase.FindAssets("t:DriveType");
            foreach (string guid in guids)
            {
                allDriveTypes.Add((DriveType)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            // Add in Drive InputConditions that don't exist in the MappingTypes (Goals)
            foreach (DriveType driveType in allDriveTypes)
            {
                MappingType mappingType = ScriptableObject.CreateInstance<MappingType>();
                mappingType.name = "AutoGeneratedFor" + driveType.name;
                InputCondition inputCondition = new InputCondition(driveType, settings.driveLevelICT, false);
                mappingType.inputConditions = new List<InputCondition>() { inputCondition };
                fakeDriveLevelMappingTypes.Add(driveType, mappingType);

                FindMatchingMappingTypes(inputCondition, mappingType, allMappingTypes);
            }

            foreach (MappingType mappingType in allMappingTypes)
            {
                foreach (InputCondition inputCondition in mappingType.inputConditions)
                {
                    FindMatchingMappingTypes(inputCondition, mappingType, allMappingTypes);
                }
            }

            Debug.Log("ICT to MappingType Lookup Dictionary Created.");
        }

        // Updates fixesInputCondition with any MappingTypes that resolve the inputCondition
        private void FindMatchingMappingTypes(InputCondition inputCondition, MappingType inputConditionMappingType, List<MappingType> allMappingTypes)
        {
            foreach (MappingType outputChangeMappingType in allMappingTypes)
            {
                // Current limitation is that MT can only be in a plan tree once (Due to relying on IC for Dicts)
                // So it makes sense to prevent this - not sure if you would ever want an MT to link to itself?
                if (outputChangeMappingType == inputConditionMappingType)
                    continue;

                // Ask outputChangeMappingType if it has any matching OutputChanges
                int outputChangeIndex = outputChangeMappingType.OutputChangeTypeMatch(inputCondition, inputConditionMappingType, allEntityTypes);
                if (outputChangeIndex != -1)
                {
                    FixInputCondition fixInputCondition = new FixInputCondition()
                    {
                        mappingType = outputChangeMappingType,
                        outputChangeIndex = outputChangeIndex
                    };

                    if (!fixesInputCondition.ContainsKey(inputCondition))
                    {
                        // New entry in dictionary - inputCondition has not been seen before
                        List<FixInputCondition> fixInputConditions = new List<FixInputCondition>() { fixInputCondition };
                        fixesInputCondition.Add(inputCondition, fixInputConditions);
                    }
                    else
                    {
                        fixesInputCondition[inputCondition].Add(fixInputCondition);
                    }
                }
            }
        }

        public List<FixInputCondition> FindMappingTypeMatches(InputCondition inputCondition)
        {
            if (fixesInputCondition.TryGetValue(inputCondition, out List<FixInputCondition> fixInputConditions))
                return fixInputConditions;
            return null;
        }

        public int AmountOfStoredEntities(TypeCategory storageCategory, TypeCategory inventoryCategory,
                                          TagType requiredTagType = null, List<Entity> possibleRelatedEntities = null)
        {
            int amount = 0;

            // TODO: A typeCategoryToEntity Dictionary would speed this up
            if (typeCategoryToIOTs != null)
            {
                foreach (InputOutputType inputOutputType in typeCategoryToIOTs[storageCategory])
                {
                    EntityType entityType = (EntityType)inputOutputType;
                    if (entityTypeToEntityInScene.TryGetValue(entityType, out List<Entity> entities))
                    {
                        foreach (Entity entity in entities)
                        {
                            if (requiredTagType == null ||
                                (entity.HasTag(requiredTagType) && (possibleRelatedEntities == null ||
                                possibleRelatedEntities.Any(x => entity.tags[requiredTagType].Select(y => y.relatedEntity).Contains(x)))))
                            amount += entity.inventoryType.GetTypeCategoryAmount(entity, inventoryCategory);
                        }
                    }
                }
            }
            return amount;
        }

        private void CreateMovementTypeList()
        {
            movementTypes = settings.movementTypes;
        }

        public void UpdateAllNavMeshes()
        {
            if (movementTypes != null)
            {
                foreach (MovementType movementType in movementTypes)
                {
                    if (movementType == null)
                    {
                        Debug.LogError("There is a Null MovementType in the Total AI Settings.  Please Fix.");
                        return;
                    }
                    movementType.UpdateNavMesh();
                }
            }
        }
    }
}