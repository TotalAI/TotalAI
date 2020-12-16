using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WorldObjectType", menuName = "Total AI/Entity Types/World Object Type", order = 0)]
    public class WorldObjectType : EntityType
    {
        // For Generalized Mapping Types
        [System.Serializable]
        public class PerBuild {
            public EntityType entityType;
            public float amount;
        }
        [Header("Build Requirements - Used by CanBuildWorldObjectICT")]
        public List<PerBuild> itemsConsumedPerBuild;

        public enum CompleteType { None, Built, Grows };
        public CompleteType completeType;
        public float pointsToComplete;

        [Header("Growth Rate: Complete points per game hour")]
        public float growthRate;
        public bool autoScale;

        public bool canBeDamaged;
        public float damageToDestroy;
        public bool removeOnFullDamage;

        [System.Serializable]
        public class State
        {
            public string name;
            public bool allowsCompletion;
            public float minComplete = -1f;
            public float maxComplete = -1f;
            public bool allowsDamage;
            public float minDamage = -1f;
            public float maxDamage = -1f;
            public bool isTimed;
            public bool timeInGameMinutes;
            public float secondsUntilNextState;
            public float gameMinutesUntilNextState;
            public string nextStateName;

            public bool hasEnterOutputChanges;
            public List<int> enterOutputChangesIndexes;
            public bool hasExitOutputChanges;
            public List<int> exitOutputChangesIndexes;
        }

        // First state is starting state
        public List<State> states;
        public List<OutputChange> statesOutputChanges;

        [Tooltip("Name of Empty GameObject that will be the parent to the Skin Prefabs.  Parent must be World Object's root GameObject")]
        public string skinGameObjectName = "Body";

        // TODO: Pull into its own file - change name - WorldObjectView?
        [System.Serializable]
        public class SkinPrefabMapping
        {
            public enum ChangeType { ReplaceGameObject, ToggleActive }
            public ChangeType changeType;

            [Tooltip("GameObject to place as child to 'Skin Game Object Name'.")]
            public GameObject prefab;

            public enum EnableType { EnableBodyChildren, EnableBodyChildrensChildren }
            public EnableType enableType;
            public enum DisableType { None, First, Previous }
            public DisableType disableType;

            public int childStartIndex;

            [Tooltip("-1 to match all Prefab Variants.")]
            public int prefabVariantIndex = -1;
            public float minComplete = -1f;
            public float maxComplete = -1f;
            public float minDamage = -1f;
            public float maxDamage = -1f;
            [Tooltip("The prefab can be dependent on the World Object's current inventory.  Leave at 0 to ignore.")]
            public List<InventorySlotCondition> inventorySlotConditions;
            [Tooltip("List of states that the World Object must be in.  Leave at 0 to ignore.")]
            public List<string> states;

            public bool Check(WorldObject worldObject)
            {
                if (prefabVariantIndex != -1 && prefabVariantIndex != worldObject.prefabVariantIndex)
                    return false;

                WorldObjectType worldObjectType = worldObject.worldObjectType;
                if (worldObjectType.completeType != CompleteType.None && minComplete != -1f && maxComplete != -1f &&
                    (worldObject.completePoints < minComplete || worldObject.completePoints > maxComplete))
                    return false;

                if (worldObjectType.canBeDamaged && minDamage != -1f && maxDamage != -1f &&
                    (worldObject.damage < minDamage || worldObject.damage > maxDamage))
                    return false;

                if (inventorySlotConditions != null && inventorySlotConditions.Count > 0)
                {
                    foreach (InventorySlotCondition inventorySlotCondition in inventorySlotConditions)
                    {
                        if (!inventorySlotCondition.Check(worldObject))
                            return false;
                    }
                }

                if (states != null && states.Count > 0 && worldObject.currentState != null && !states.Contains(worldObject.currentState.name))
                    return false;

                return true;
            }

            public GameObject NextToBeActivated(WorldObject worldObject)
            {
                GameObject skinGameObject = null;
                foreach (Transform child in worldObject.transform)
                {
                    if (child.name == worldObject.worldObjectType.skinGameObjectName)
                    {
                        skinGameObject = child.gameObject;
                    }
                }

                if (skinGameObject == null)
                {
                    Debug.LogError(worldObject.name + ": Unable to find a child name that equals \"" + worldObject.worldObjectType.skinGameObjectName +
                                    "\" as specificied in its WorldObjectType.skinGameObjectName");
                    return null;
                }

                int currentIndex = childStartIndex + (int)worldObject.completePoints + 1;

                // Find GameObject to activate
                int index = 0;
                GameObject gameObject = null;
                if (enableType == EnableType.EnableBodyChildren)
                {
                    foreach (Transform child in skinGameObject.transform)
                    {
                        if (index == currentIndex)
                        {
                            gameObject = child.gameObject;
                            break;
                        }
                        ++index;
                    }
                }
                else
                {
                    bool foundGameObject = false;
                    foreach (Transform parent in skinGameObject.transform)
                    {
                        foreach (Transform child in parent.transform)
                        {
                            if (index == currentIndex)
                            {
                                gameObject = child.gameObject;
                                foundGameObject = true;
                                break;
                            }
                            ++index;
                        }
                        if (foundGameObject)
                            break;
                    }
                }

                return gameObject;
            }
        }

        public List<SkinPrefabMapping> skinPrefabMappings;

        public enum AddInventoryTiming { Created, Completed }
        public AddInventoryTiming addDefaultInventoryTiming;
        
        public List<WOTInventoryRecipe> recipes;

        public override GameObject CreateEntity(int prefabVariantIndex, Vector3 position, Quaternion rotation, Vector3 scale, Entity creator)
        {
            GameObject prefab = prefabVariants[prefabVariantIndex];
            prefab.SetActive(false);

            GameObject newGameObject = Instantiate(prefab, position, rotation);
            newGameObject.transform.localScale = scale;

            WorldObject worldObject = newGameObject.GetComponent<WorldObject>();

            // Set transform scale to 0.05 if this World Object uses auto scale
            // TODO: This value should be settable
            if (autoScale && (states.Count == 0 || states[0].allowsCompletion))
                worldObject.transform.localScale = new Vector3(.05f, .05f, .05f);

            newGameObject.SetActive(true);
            prefab.SetActive(true);

            TotalAIManager.manager.UpdateAllNavMeshes();

            return newGameObject;
        }

        // WorldObject is being created as an Entity's inventory
        // TODO: Should this just be one method in InventoryType?
        public override Entity CreateEntityInInventory(Entity entity, int prefabVariantIndex, InventorySlot inventorySlot)
        {
            if (inventorySlot.slotType != InventorySlot.SlotType.Skinned)
            {
                GameObject parentGameObject = entity.GetSlotMapping(inventorySlot, out bool makeDisabled);

                if (prefabVariants == null || prefabVariants.Count < 1)
                {
                    Debug.LogError("No prefabVariants for " + name + ".  Unable to create WorldObjectType In Inventory: " + name +
                                   " in Entity: " + entity.name);
                    return null;
                }
                if (prefabVariantIndex < 0 || prefabVariantIndex > prefabVariants.Count - 1)
                {
                    Debug.LogError("Invalid prefabVariantIndex (" + prefabVariantIndex + ") for " + name + ".  Unable to create " +
                                   "WorldObjectType In Inventory: " + name + " in Entity: " + entity.name);
                    return null;
                }

                GameObject prefabVariant = prefabVariants[prefabVariantIndex];
                prefabVariant.SetActive(false);

                GameObject newGameObject = Instantiate(prefabVariant, parentGameObject.transform);
                newGameObject.layer = LayerMask.NameToLayer("Default");

                if (inventorySlot.slotType != InventorySlot.SlotType.Invisible && !makeDisabled)
                    newGameObject.SetActive(true);

                prefabVariant.SetActive(true);

                Entity newEntity = newGameObject.GetComponent<Entity>();
                newEntity.inEntityInventory = entity;

                if (inventorySlot.slotType != InventorySlot.SlotType.Invisible)
                {
                    GameObject gameObject = GetTransformMapping(inventorySlot, entity, newEntity);
                    newGameObject.transform.localPosition = gameObject.transform.position;
                    newGameObject.transform.localRotation = gameObject.transform.rotation;
                    newGameObject.transform.localScale = gameObject.transform.localScale;
                }

                return newEntity;

            }            
            else
            {
                // TODO: Skinned inventory slot
            }

            return null;
        }
        
        public GameObject GetCurrentSkinPrefab(WorldObject worldObject, out int index)
        {
            index = -1;
            for (int i = 0; i < skinPrefabMappings.Count; i++)
            {
                SkinPrefabMapping skinPrefabMapping = skinPrefabMappings[i];

                if (skinPrefabMapping.Check(worldObject))
                {
                    index = i;
                    return skinPrefabMapping.prefab;
                }
            }

            return null;
        }
        
        public List<string> StateNames()
        {
            return states.Select(x => x.name).ToList();
        }
    }
}