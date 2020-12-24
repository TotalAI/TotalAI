using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public abstract class InventoryType : ScriptableObject
    {
        public bool usesItemAttributeTypeModifiers = true;
        public bool usesItemActionSkillModifiers = true;
        public bool usesItemSoundEffects = true;
        public bool usesItemVisualEffects = true;

        [Serializable]
        public class Item
        {
            public Entity entity;
            public InventorySlot inventorySlot;
        }
        public Dictionary<Entity, Dictionary<EntityType, List<Item>>> inventoryByType;
        public Dictionary<Entity, Dictionary<InventorySlot, List<Item>>> inventoryBySlot;
        public Dictionary<Entity, Dictionary<InventorySlot, int>> inventoryBySlotCount;

        public abstract Vector3 FindDropLocation(Entity entity, Entity entityDropped, InventorySlot inventorySlot);
        public abstract void ChangeSkin(WorldObject worldObject, GameObject newPrefab, int skinPrefabIndex);
        public abstract void Thrown(Entity thrownEntity, Entity thrownByEntity, InventorySlot inventorySlot, Vector3 target, float forceStrength);

        public virtual void ChangeSkinIfNeeded(WorldObject worldObject)
        {
            // TODO: Maybe Agents do want to use it?  Move it to EntityType?
            GameObject newPrefab = worldObject.worldObjectType.GetCurrentSkinPrefab(worldObject, out int skinPrefabIndex);

            if (skinPrefabIndex != -1 &&
                (worldObject.currentSkinPrefabIndex != skinPrefabIndex ||
                worldObject.worldObjectType.skinPrefabMappings[skinPrefabIndex].changeType == WorldObjectType.SkinPrefabMapping.ChangeType.ToggleActive))
            {
                ChangeSkin(worldObject, newPrefab, skinPrefabIndex);
            }
        }

        public virtual void OnEnable()
        {
            inventoryByType = new Dictionary<Entity, Dictionary<EntityType, List<Item>>>();
            inventoryBySlot = new Dictionary<Entity, Dictionary<InventorySlot, List<Item>>>();
            inventoryBySlotCount = new Dictionary<Entity, Dictionary<InventorySlot, int>>();
        }

        public virtual void SetupEntity(Entity entity)
        {
            inventoryByType[entity] = new Dictionary<EntityType, List<Item>>();
            inventoryBySlot[entity] = new Dictionary<InventorySlot, List<Item>>();
            inventoryBySlotCount[entity] = new Dictionary<InventorySlot, int>();
        }

        public virtual void Reset(Entity entity)
        {
            EntityType entityType = entity.entityType;

            inventoryByType[entity] = new Dictionary<EntityType, List<Item>>();
            inventoryBySlot[entity] = new Dictionary<InventorySlot, List<Item>>();
            inventoryBySlotCount[entity] = new Dictionary<InventorySlot, int>();

            foreach (InventorySlot inventorySlot in entityType.inventorySlots)
            {
                inventoryBySlot[entity].Add(inventorySlot, new List<Item>());
                inventoryBySlotCount[entity].Add(inventorySlot, 0);
            }

            if (!(entity is WorldObject worldObject) || worldObject.worldObjectType.addDefaultInventoryTiming == WorldObjectType.AddInventoryTiming.Created)
            {
                foreach (EntityType.DefaultInventory defaultInventory in entityType.defaultInventory)
                {
                    if (UnityEngine.Random.Range(0f, 100f) <= defaultInventory.probability)
                    {
                        if (defaultInventory.amountCurve == null)
                        {
                            Debug.LogError(entityType.name + ": Missing AmountCurve for Default Inventory.");
                            return;
                        }

                        int numToAdd = Mathf.RoundToInt(defaultInventory.amountCurve.Eval(UnityEngine.Random.Range(0f, 1f)));
                        int numAdded = Add(entity, defaultInventory.entityType, defaultInventory.prefabVariantIndex,
                                           defaultInventory.inventorySlot, numToAdd);

                        if (numAdded != numToAdd)
                            Debug.LogError(entity.name + ": Failed to add default inventory - added " + numAdded + "/" +
                                           numToAdd + " of " + defaultInventory.entityType.name);

                        // TODO: Add in EntityModifiers that an Entity might add

                    }
                }
            }

            if (entity.entityTypeOverrides != null)
            {
                foreach (EntityTypeOverride entityTypeOverride in entity.entityTypeOverrides)
                {
                    foreach (EntityTypeOverride.OverrideInventoryItem overrideInventoryItem in entityTypeOverride.overrideInventory)
                    {
                        if (overrideInventoryItem.overrideType == EntityOverrideType.AddOrReplace)
                        {
                            if (UnityEngine.Random.Range(0f, 100f) <= overrideInventoryItem.probability)
                            {
                                int numAdded = Add(entity, overrideInventoryItem.type, overrideInventoryItem.prefabVariantIndex,
                                                   overrideInventoryItem.inventorySlot, overrideInventoryItem.numberOfTypes);

                                if (numAdded != overrideInventoryItem.numberOfTypes)
                                    Debug.LogError(entity.name + ": Failed to add default inventory - added " + numAdded + "/" +
                                                   overrideInventoryItem.numberOfTypes + " of " + overrideInventoryItem.type.name);
                            }
                        }
                        else
                        {
                            List<Item> items = Remove(entity, overrideInventoryItem.type, overrideInventoryItem.numberOfTypes, false);
                            foreach (Item item in items)
                            {
                                item.entity.DestroySelf(null);
                            }
                        }
                    }
                }
            }
        }

        // Returns the main inventorySlot in out InventorySlot chosenInventorySlot - will be null if it couldn't find room
        // Return Dict maps the slots that need to be cleared to the amount of items in the slot that will be needed
        // ignoreClearSlots if this is called just to see if there is room if items are cleared out
        public virtual Dictionary<InventorySlot, int> FindInventorySlotsToClear(Entity entity, EntityType entityType,
                                                                                out InventorySlot chosenInventorySlot,
                                                                                int numOfEntities = 1, bool ignoreClearSlots = false)
        {
            chosenInventorySlot = null;
            List<InventorySlot> inventorySlots = entityType.canBeInInventorySlots.Select(x => x.inventorySlot).ToList();
            List<InventorySlot> possibleSlots = inventorySlots.Intersect(inventoryBySlot[entity].Keys).ToList();

            if (possibleSlots.Count == 0)
            {
                Debug.LogError(entity.name + ": ClearSlotsAndAdd - unable to find a slot to clear.");
                return null;
            }

            EntityType.CanBeInInventorySlot canBeIn = null;
            Dictionary<InventorySlot, int> slotsToClear = null;
            foreach (InventorySlot inventorySlot in possibleSlots)
            {
                if (!ignoreClearSlots)
                    slotsToClear = new Dictionary<InventorySlot, int>();
                canBeIn = entityType.canBeInInventorySlots.Find(x => x.inventorySlot == inventorySlot);
                if (canBeIn.numLocations <= inventorySlot.maxNumberEntities * numOfEntities)
                {
                    if (!ignoreClearSlots)
                        slotsToClear[inventorySlot] = canBeIn.numLocations * numOfEntities;

                    // Deal with entityToAdd if it has multi-slot or multi-location requirements
                    if (canBeIn.otherInventorySlots != null && canBeIn.otherInventorySlots.Count > 0)
                    {
                        bool failed = false;
                        for (int i = 0; i < canBeIn.otherInventorySlots.Count; i++)
                        {
                            InventorySlot otherInventorySlot = canBeIn.otherInventorySlots[i];
                            int otherNumLocations = canBeIn.otherInventorySlotsNumLocations[i];
                            if (!inventoryBySlot[entity].ContainsKey(otherInventorySlot) ||
                                otherNumLocations > otherInventorySlot.maxNumberEntities * numOfEntities)
                            {
                                failed = true;
                                break;
                            }
                            if (!ignoreClearSlots)
                                slotsToClear[otherInventorySlot] = otherNumLocations * numOfEntities;
                        }
                        if (failed)
                            continue;
                    }
                    chosenInventorySlot = inventorySlot;
                    break;
                }
            }

            return slotsToClear;
        }

        // Will clear out needed slots for entity
        public virtual InventorySlot ClearSlotsAndAdd(Entity entity, Entity entityToAdd)
        {
            Dictionary<InventorySlot, int> slotsToClear = FindInventorySlotsToClear(entity, entityToAdd.entityType, out InventorySlot inventorySlot);

            if (inventorySlot == null)
            {
                Debug.LogError("ClearSlotsAndAdd is trying to put " + entityToAdd.name + " in " + entity.name +
                               " and it couldn't find a slot to clear.");
                return null;
            }

            foreach (KeyValuePair<InventorySlot, int> slotWithCount in slotsToClear)
            {
                while (CapacityRemainingInSlot(entity, slotWithCount.Key) < slotWithCount.Value)
                {
                    Item item = Remove(entity, slotWithCount.Key);
                    if (item != null)
                        Dropped(item.entity, slotWithCount.Key);
                    else
                        break;
                }
            }

            Add(entity, entityToAdd, inventorySlot);
            return inventorySlot;
        }

        // Single point of adding inventory - no other location should add an entity to inventory
        public virtual InventorySlot Add(Entity entity, Entity entityToAdd, InventorySlot inventorySlot = null, bool createdInInventory = false)
        {
            if (inventorySlot == null)
            {
                inventorySlot = FindInventorySlot(entity, entityToAdd.entityType);
                if (inventorySlot == null)
                {
                    Debug.Log(entity.name + ": Inventory unable to add " + entityToAdd.name + " - Could not find an InventorySlot.");
                    return null;
                }
            }
            
            Item inventoryItem = new Item()
            {
                inventorySlot = inventorySlot,
                entity = entityToAdd
            };

            inventoryBySlot[entity][inventorySlot].Add(inventoryItem);
            UpdateSlotCount(entity, entityToAdd, inventorySlot, true);

            if (inventoryByType[entity].ContainsKey(entityToAdd.entityType))
            {
                inventoryByType[entity][entityToAdd.entityType].Add(inventoryItem);
            }
            else
            {
                inventoryByType[entity].Add(entityToAdd.entityType, new List<Item>() { inventoryItem });
            }

            if (!createdInInventory)
                PickedUp(entityToAdd, entity, inventorySlot);

            // If this inventory is for a WorldObject - adding the entity may have triggered a recipe and/or skin change
            if (entity is WorldObject worldObject)
            {
                worldObject.CheckRecipes(entityToAdd.entityType);
                ChangeSkinIfNeeded(worldObject);
            }

            // If an Agent is being put into inventory might need to make somes changes to the Agent
            if (entityToAdd is Agent agent)
            {
                AgentIntoInventory(agent);
            }

            // Disable all colliders - Not sure if this is best way - might still want some items to have trigger colliders
            // TODO: Trigger collider staying active shoud be based on EntityType and InventorySlot - or maybe just InventorySlot?
            ActivateColliders(entityToAdd, false);

            // Change to Inventory layer so entity items won't get considered in NavMesh updates
            SetLayerRecursively(entityToAdd, entityToAdd.gameObject, LayerMask.NameToLayer("Inventory"), false);

            return inventorySlot;
        }

        // On an Add or Remove this is resposible for updating the slot counts - takes into account multi-slot multi-location items
        protected virtual void UpdateSlotCount(Entity entity, Entity inInventoryEntity, InventorySlot inventorySlot, bool beingAdded)
        {
            EntityType.CanBeInInventorySlot canBeSlot = inInventoryEntity.entityType.canBeInInventorySlots.Find(x => x.inventorySlot == inventorySlot);

            if (canBeSlot == null)
            {
                Debug.LogError(entity.name + ": Trying to put " + inInventoryEntity.name + " in inventory. Missing can be in inventory slot: " +
                          inventorySlot.name);
                return;
            }

            if (beingAdded)
            {
                inventoryBySlotCount[entity][inventorySlot] += canBeSlot.numLocations;
                if (canBeSlot.otherInventorySlots != null)
                {
                    for (int i = 0; i < canBeSlot.otherInventorySlots.Count; i++)
                    {
                        inventoryBySlotCount[entity][canBeSlot.otherInventorySlots[i]] += canBeSlot.otherInventorySlotsNumLocations[i];
                    }
                }
            }
            else
            {
                inventoryBySlotCount[entity][inventorySlot] -= canBeSlot.numLocations;
                if (canBeSlot.otherInventorySlots != null)
                {
                    for (int i = 0; i < canBeSlot.otherInventorySlots.Count; i++)
                    {
                        inventoryBySlotCount[entity][canBeSlot.otherInventorySlots[i]] -= canBeSlot.otherInventorySlotsNumLocations[i];
                    }
                }
            }
        }

        protected virtual void AgentIntoInventory(Agent agent)
        {
            agent.movementType.Disable(agent);
        }

        protected virtual void AgentOutOfInventory(Agent agent)
        {
            agent.movementType.Enable(agent);
        }

        protected virtual void ActivateColliders(Entity entity, bool activate)
        {
            foreach (Collider c in entity.GetComponentsInChildren<Collider>())
            {
                c.enabled = activate;
            }
        }

        public virtual int Add(Entity entity, EntityType entityType, int prefabVariantIndex, InventorySlot inventorySlot = null, int amount = 1)
        {
            if (inventorySlot == null)
            {
                // All or nothing on amount - no partial adds
                inventorySlot = FindInventorySlot(entity, entityType, amount);
                if (inventorySlot == null)
                    return 0;
            }

            int numAdded = 0;
            for (int i = 0; i < amount; i++)
            {
                Entity entityToAdd = entityType.CreateEntityInInventory(entity, prefabVariantIndex, inventorySlot);
                if (Add(entity, entityToAdd, inventorySlot, true))
                    numAdded++;
            }
            return numAdded;
        }

        public virtual int Add(Entity entity, List<Item> items)
        {
            int numAdded = 0;
            foreach (Item item in items)
            {
                if (Add(entity, item.entity) != null)
                    ++numAdded;
            }

            return numAdded;
        }

        public virtual int Add(Entity entity, List<Entity> entities)
        {
            int numAdded = 0;
            foreach (Entity entityToAdd in entities)
            {
                if (Add(entity, entityToAdd) != null)
                    ++numAdded;
            }

            return numAdded;
        }

        public virtual Item Remove(Entity entity, Entity entityToRemove, bool failSilently = false)
        {
            if (inventoryByType[entity].TryGetValue(entityToRemove.entityType, out List<Item> inventoryItems))
            {
                int index = inventoryItems.FindIndex(x => x.entity == entityToRemove);
                if (index != -1)
                {
                    Item item = inventoryItems[index];
                    Remove(entity, item);
                    return item;
                }
                else if (!failSilently)
                {
                    Debug.LogError("Tried to remove an Entity (" + entityToRemove.name + ") that is not in " + entity.name + "'s inventory.");
                }
            }
            else if (!failSilently)
            {
                Debug.LogError("Tried to remove an Entity (" + entityToRemove.name + ") that is not in " + entity.name + "'s inventory.");
            }
            return null;
        }

        public virtual List<Item> Remove(Entity entity, TypeGroup grouping, int amount, bool searchNested, bool failSilently = false)
        {
            List<EntityType> entityTypes = grouping.GetMatches(GetAllEntityTypes(entity));

            foreach (EntityType entityType in entityTypes)
            {
                if (GetEntityTypeAmount(entity, entityType) >= amount)
                {
                    // Found an EntityType with enough amount - remove them
                    // TODO: preference over which EntityType to remove - could InventoryTFs be used?
                    return Remove(entity, entityType, amount, searchNested);
                }
            }
            return null;
        }

        public virtual List<Item> Remove(Entity entity, EntityType entityType, int amount, bool searchNested, bool failSilently = false)
        {
            if (inventoryByType[entity].TryGetValue(entityType, out List<Item> inventoryItems))
            {
                if (inventoryItems.Count >= amount)
                {
                    // TODO: Does it matter which ones get removed?  Eventually consider TFs for this?
                    List<Item> removedItems = inventoryItems.GetRange(inventoryItems.Count - amount, amount);
                    foreach (Item item in removedItems)
                    {
                        Remove(entity, item);
                    }
                    return removedItems;
                }
                else if (!failSilently)
                {
                    Debug.Log(entity.name + ":Tried to remove " + amount + " EntityType(s) (" + entityType.name + ") but only " + inventoryItems.Count +
                              " are in " + entity.name + "'s inventory.  Only searched top level of inventory.");
                }
            }
            else if (searchNested)
            {
                // Need to search any entityType that can hold inventory
                List<Entity> allEntities = GetAllEntities(entity, true);
                List<Entity> entities = allEntities.FindAll(x => x.entityType == entityType);
                if (entities.Count >= amount)
                {
                    List<Item> results = new List<Item>();
                    for (int i = entities.Count - 1; i >= entities.Count - amount; i--)
                    {
                        Entity entityToRemove = entities[i];
                        results.Add(entityToRemove.inEntityInventory.inventoryType.Remove(entityToRemove.inEntityInventory, entityToRemove));
                    }
                    return results;
                }
                else if (!failSilently)
                {
                    Debug.Log(entity.name + ":Tried to remove " + amount + " EntityType(s) (" + entityType.name + ") but only " + entities.Count +
                              " are in " + entity.name + "'s inventory.  Did full nested search.");
                }
            }
            else if (!failSilently)
            {
                Debug.Log(entity.name + ":Tried to remove " + amount + " EntityType(s) (" + entityType.name + ") that are not in " +
                          entity.name + "'s inventory.");
            }

            return null;
        }

        public virtual Item Remove(Entity entity, InventorySlot inventorySlot, bool failSilently = false)
        {
            if (inventoryBySlot[entity].TryGetValue(inventorySlot, out List<Item> inventoryItems) && inventoryItems.Count > 0)
            {
                Item item = inventoryItems[inventoryItems.Count - 1];
                Remove(entity, item);
                return item;
            }
            return null;
        }

        protected virtual void Remove(Entity entity, Item item)
        {
            List<Item> itemsInSlot = inventoryBySlot[entity][item.inventorySlot];
            itemsInSlot.Remove(item);
            UpdateSlotCount(entity, item.entity, item.inventorySlot, false);
            ActivateColliders(item.entity, true);

            // Set all layers to correct layer on removal and make sure to keep removed entity's inventory on the "Inventory" layer
            SetLayerRecursively(item.entity, item.entity.gameObject, EntityType.GetLayer(item.entity, true), true);

            inventoryByType[entity][item.entity.entityType].Remove(item);

            // If this was the last entity of this EntityType - remove the EntityType from inventoryByType Dictionary
            if (inventoryByType[entity][item.entity.entityType].Count == 0)
                inventoryByType[entity].Remove(item.entity.entityType);

            // See if there is disabled inventory that should be moved to newly opened spot
            if (item.inventorySlot.extraBecomesInvisible)
            {
                // Disabled inventory goes in last location
                Entity.InventorySlotMapping inventorySlotMapping = entity.inventorySlotMappings.Find(x => x.inventorySlot == item.inventorySlot);
                int numLocations = inventorySlotMapping.locations.Count;
                if (itemsInSlot.Count > numLocations - 1)
                {
                    foreach (Transform child in inventorySlotMapping.locations[numLocations - 1].transform)
                    {
                        if (!child.gameObject.activeInHierarchy)
                        {
                            // Found one - move it to the empty location
                            foreach (GameObject location in inventorySlotMapping.locations)
                            {
                                if (location.transform.childCount == 0 || location.transform.GetChild(0).GetComponent<Entity>() == item.entity)
                                {
                                    child.parent = location.transform;
                                    GameObject gameObject = item.entity.entityType.GetTransformMapping(item.inventorySlot, entity, item.entity);
                                    child.localPosition = gameObject.transform.position;
                                    child.localRotation = gameObject.transform.rotation;
                                    child.localScale = gameObject.transform.localScale;
                                    child.gameObject.SetActive(true);
                                    break;
                                }
                            }
                            break;
                        }
                    } 
                }
            }

            if (entity is WorldObject worldObject)
            {
                ChangeSkinIfNeeded(worldObject);
            }

            // If an Agent is being removed from inventory might need to make somes changes to the Agent
            if (item.entity is Agent agent)
            {
                AgentOutOfInventory(agent);
            }
        }
        
        public virtual void PickedUp(Entity entityPickedUp, Entity byEntity, InventorySlot inventorySlot)
        {
            // Remove Layer so its not picked up by Raycasts
            // TODO: Inventory Add should handle this like Remove does
            entityPickedUp.gameObject.layer = LayerMask.NameToLayer("Default");
            entityPickedUp.inEntityInventory = byEntity;
            GameObject parentGameObject = byEntity.GetSlotMapping(inventorySlot, out bool makeDisabled, true);
            entityPickedUp.transform.parent = parentGameObject.transform;

            if (inventorySlot.slotType == InventorySlot.SlotType.Invisible || makeDisabled)
            {
                // TODO: Might be better to leave WOs Active too but disable the renderer
                if (entityPickedUp is Agent agent)
                    agent.GetComponentInChildren<Renderer>().enabled = false;
                else
                    entityPickedUp.gameObject.SetActive(false);
            }
            else if (inventorySlot.slotType == InventorySlot.SlotType.OwnerInvisible)
            {
                byEntity.GetComponentInChildren<Renderer>().enabled = false;
            }
            else
            {
                entityPickedUp.gameObject.SetActive(true);
            }

            // Get the new position and rotation from the EntityType
            if (inventorySlot.slotType != InventorySlot.SlotType.Invisible)
            {
                GameObject gameObject = entityPickedUp.entityType.GetTransformMapping(inventorySlot, byEntity, entityPickedUp);
                entityPickedUp.transform.localPosition = gameObject.transform.position;
                entityPickedUp.transform.localRotation = gameObject.transform.rotation;
                entityPickedUp.transform.localScale = gameObject.transform.localScale;
            }
            else
            {
                entityPickedUp.transform.localPosition = Vector3.zero;
            }
        }
        
        public virtual void Dropped(Entity entityDropped, InventorySlot inventorySlot)
        {
            // TODO: Handle Agents
            //gameObject.layer = LayerMask.NameToLayer("WorldObject");

            // Unparent and place near Entity
            Entity droppedBy = entityDropped.inEntityInventory;
            entityDropped.inEntityInventory = null;

            // TODO: Want to know if Entity cuts out NavMesh
            entityDropped.transform.parent = null;

            // Reset rotation to prefab rotation - this should be the desired rotation for an Entity when not in inventory
            GameObject prefabVariant = entityDropped.PrefabVariant();
            if (prefabVariant == null)
                Debug.LogError(name + ": Trying to drop but it is missing prefabVariant index #" + entityDropped.prefabVariantIndex +
                               " - needed to correctly set rotation of Entity on ground.");
            entityDropped.transform.rotation = prefabVariant.transform.rotation;
            entityDropped.transform.localScale = prefabVariant.transform.localScale;

            if (entityDropped is Agent agent)
            {
                // Agents don't get disabled - just made invisible if they were in an invisible slot
                agent.GetComponentInChildren<Renderer>().enabled = true;
            }
            else
            {
                entityDropped.gameObject.SetActive(true);
            }

            if (inventorySlot.slotType == InventorySlot.SlotType.OwnerInvisible &&
                entityDropped.inventoryType.GetInventorySlotAmount(entityDropped, inventorySlot) == 0)
            {
                droppedBy.GetComponentInChildren<Renderer>().enabled = true;
            }

            entityDropped.transform.position = droppedBy.inventoryType.FindDropLocation(droppedBy, entityDropped, inventorySlot);
            
        }

        // Priority is currently order of CanBeInInventorySlots
        // Returns null if there is no InventorySlot available
        public virtual InventorySlot FindInventorySlot(Entity entity, EntityType entityType, int numberOfEntities = 1)
        {
            if (!entityType.CanBeInInventory())
            {
                Debug.Log("FindInventorySlot: Trying to put " + numberOfEntities + " " + entityType.name + " into " + entity.name + ". " +
                          "The EntityType can't be in inventory.");
                return null;
            }

            if (!entity.entityType.CanHaveInventory())
            {
                Debug.Log("FindInventorySlot: Trying to put " + numberOfEntities + " " + entityType.name + " into " + entity.name + ". " +
                         "The Entity can't have inventory.");
                return null;
            }
            
            foreach (EntityType.CanBeInInventorySlot canBeInSlot in entityType.canBeInInventorySlots)
            {
                if (inventoryBySlot[entity].ContainsKey(canBeInSlot.inventorySlot) &&
                    CapacityRemainingInSlot(entity, canBeInSlot.inventorySlot) >= canBeInSlot.numLocations * numberOfEntities)
                {
                    // Check to see if this item takes up any other slots
                    if (canBeInSlot.otherInventorySlots != null)
                    {
                        bool hasRoom = true;
                        for (int i = 0; i < canBeInSlot.otherInventorySlots.Count; i++)
                        {
                            InventorySlot otherInventorySlot = canBeInSlot.otherInventorySlots[i];
                            if (!inventoryBySlot[entity].ContainsKey(otherInventorySlot) ||
                                CapacityRemainingInSlot(entity, otherInventorySlot) <
                                canBeInSlot.otherInventorySlotsNumLocations[i] * numberOfEntities)
                            {
                                hasRoom = false;
                                break;
                            }
                        }
                        if (!hasRoom)
                            continue;
                    }

                    return canBeInSlot.inventorySlot;
                }
            }
            Debug.Log("FindInventorySlot: Trying to put " + numberOfEntities + " " + entityType.name + " into " + entity.name + ". " +
                      "Unable to find an open InventorySlot for that amount of EntityType.");
            return null;
        }

        protected virtual int CapacityRemainingInSlot(Entity entity, InventorySlot inventorySlot)
        {
            return inventorySlot.maxNumberEntities == -1 ?
                        int.MaxValue : inventorySlot.maxNumberEntities - inventoryBySlotCount[entity][inventorySlot];
        }

        // Get entityToMove - remove it and then add it back in at new spot
        // This can handle the entityToMove being nested but will only move it to a an open slot in entity (not nested)
        public virtual bool MoveInventorySlot(Entity entity, Entity entityToMove, bool makeEquipped, bool includeNested = false)
        {
            Item itemToMove = GetInventoryItem(entity, entityToMove, includeNested);

            InventorySlot inventorySlot = null;
            if (makeEquipped)
            {
                // Find an Inventory Slot that makes this item equipped and also has capacity
                List<InventorySlot> possibleSlots = EquippedSlots(entity, entityToMove.entityType);
                foreach (InventorySlot possibleSlot in possibleSlots)
                {
                    if (inventoryBySlot[entity].ContainsKey(possibleSlot) && CapacityRemainingInSlot(entity, possibleSlot) > 0)
                    {
                        inventorySlot = possibleSlot;
                        break;
                    }
                }
                if (inventorySlot == null)
                {
                    Debug.LogError(entity.name + ": MoveInventorySlot was unable to find an equipped inventory slot for " + entityToMove.name);
                    return false;
                }
            }
            else
            {
                inventorySlot = FindInventorySlot(entity, entityToMove.entityType);
                if (inventorySlot == null)
                {
                    Debug.LogError(entity.name + ": MoveInventorySlot was unable to find an inventory slot for " + entityToMove.name);
                    return false;
                }
            }

            // Remove item - might be in a nested item
            if (includeNested)
            {
                Entity itemToMoveInEntity = itemToMove.entity.inEntityInventory;
                itemToMoveInEntity.inventoryType.Remove(itemToMoveInEntity, itemToMove);
            }
            else
            {
                Remove(entity, itemToMove);
            }

            InventorySlot actualInventorySlot = entity.inventoryType.Add(entity, entityToMove, inventorySlot);
            if (actualInventorySlot == null)
            {
                Debug.LogError(entity.name + ": MoveInventorySlot Add failed to an inventory slot for " + entityToMove.name);
                return false;
            }
            
            return true;
        }

        // Finds the inputs and destroys them and then creates the outputs
        public virtual bool Convert(Entity entity, List<EntityType> inputEntityTypes, List<int> inputAmounts,
                            List<EntityType> outputEntityTypes, List<int> outputAmounts)
        {
            // TODO: Make sure Entity can hold all of the outputs



            // Destroy inputs
            List<Item> removedItems = new List<Item>();
            for (int i = 0; i < inputEntityTypes.Count; i++)
            {
                removedItems.AddRange(Remove(entity, inputEntityTypes[i], inputAmounts[i], true));
            }

            // Create outputs
            for (int i = 0; i < outputEntityTypes.Count; i++)
            {
                // TODO: Add in prefabVariantIndex
                int numAdded = Add(entity, outputEntityTypes[i], 0, null, outputAmounts[i]);

                if (numAdded != outputAmounts[i])
                {
                    Debug.LogError(entity.name + ": Failed to Add outputs (" + outputEntityTypes[i].name + ") during a Convert inventory - " +
                                   "Added " + numAdded + " out of " + outputAmounts[i]);
                    
                    //TODO: Undo the remove and any adds already done

                }
            }

            foreach (Item item in removedItems)
            {
                item.entity.DestroySelf(null);
            }

            return true;
        }

        public virtual Item GetInventoryItem(Entity entity, Entity entityToFind, bool includeNested = false)
        {
            if (!includeNested)
            {
                if (inventoryByType[entity].TryGetValue(entityToFind.entityType, out List<Item> items))
                    return items.Find(x => x.entity == entityToFind);
            }
            else
            {
                List<Item> allItems = GetAllInventoryItems(entity, true);
                return allItems.Find(x => x.entity == entityToFind);
            }
            return null;
        }

        public virtual bool EntityInInventory(Entity entity, Entity entityToFind)
        {
            return inventoryByType[entity].TryGetValue(entityToFind.entityType, out List<Item> items) && items.Exists(x => x.entity == entityToFind);
        }

        public virtual bool EntityTypeInSlot(Entity entity, EntityType entityType, InventorySlot inventorySlot)
        {
            return inventoryByType[entity].TryGetValue(entityType, out List<Item> items) && items.Exists(x => x.inventorySlot == inventorySlot);
        }

        public virtual bool EntityTypeInVisibleSlot(Entity entity, EntityType entityType)
        {
            return inventoryByType[entity].TryGetValue(entityType, out List<Item> items) &&
                   items.Exists(x => x.inventorySlot.slotType != InventorySlot.SlotType.Invisible);
        }

        public virtual int GetEntityTypeAmount(Entity entity, EntityType entityType, InventorySlot inventorySlot = null,
                                               int prefabVariantIndex = -1, bool includeNested = false)
        {
            if (!includeNested)
            {
                if (inventoryByType[entity].TryGetValue(entityType, out List<Item> items))
                {
                    if (inventorySlot != null && prefabVariantIndex != -1)
                        return items.Where(x => x.inventorySlot == inventorySlot && x.entity.prefabVariantIndex == prefabVariantIndex).Count();
                    else if (inventorySlot != null)
                        return items.Where(x => x.inventorySlot == inventorySlot).Count();
                    else if (prefabVariantIndex != -1)
                        return items.Where(x => x.entity.prefabVariantIndex == prefabVariantIndex).Count();
                    return items.Count;
                }
            }
            else
            {
                List<Entity> results = GetAllEntities(entity, entityType, includeNested);
                if (inventorySlot != null && prefabVariantIndex != -1)
                    return results.Where(x => x.inEntityInventory.inventoryType.GetEntitySlot(x.inEntityInventory, x) == inventorySlot &&
                                              x.prefabVariantIndex == prefabVariantIndex).Count();
                else if (inventorySlot != null)
                    return results.Where(x => x.inEntityInventory.inventoryType.GetEntitySlot(x.inEntityInventory, x) == inventorySlot).Count();
                else if (prefabVariantIndex != -1)
                    return results.Where(x => x.prefabVariantIndex == prefabVariantIndex).Count();
                return results.Count;
            }
            return 0;
        }

        public virtual int GetInventorySlotAmount(Entity entity, InventorySlot inventorySlot)
        {
            if (inventoryBySlot[entity].TryGetValue(inventorySlot, out List<Item> items))
                return items.Count;
            return 0;
        }

        public virtual int GetTypeCategoryAmount(Entity entity, TypeCategory typeCategory, InventorySlot inventorySlot = null)
        {
            int amount = 0;
            foreach (KeyValuePair<EntityType, List<Item>> items in inventoryByType[entity])
            {
                if (items.Key.typeCategories.Contains(typeCategory))
                {
                    if (inventorySlot != null)
                        amount += items.Value.Where(x => x.inventorySlot == inventorySlot).Count();
                    else
                        amount += items.Value.Count;
                }
            }
            return amount;
        }

        public virtual int GetTypeGroupAmount(Entity entity, TypeGroup typeGroup, InventorySlot inventorySlot = null)
        {
            int amount = 0;
            List<EntityType> entityTypes = typeGroup.GetMatches(inventoryByType[entity].Keys.ToList());
            foreach (EntityType entityType in entityTypes)
            {
                if (inventorySlot != null)
                    amount += inventoryByType[entity][entityType].Where(x => x.inventorySlot == inventorySlot).Count();
                else
                    amount += inventoryByType[entity][entityType].Count;
            }
            return amount;
        }

        public virtual List<Item> GetAllInventoryItems(Entity entity, bool includeNested = false)
        {
            List<Item> results = new List<Item>();

            foreach (List<Item> items in inventoryBySlot[entity].Values)
            {
                foreach (Item item in items)
                {
                    if (includeNested && item.entity.entityType.CanHaveInventory())
                    {
                        results.AddRange(item.entity.inventoryType.GetAllInventoryItems(item.entity, true));
                    }
                    results.Add(item);
                }
            }
            return results;
        }

        public virtual List<Entity> GetAllEntities(Entity entity, bool includeNested = false)
        {
            List<Entity> results = new List<Entity>();

            foreach (List<Item> items in inventoryBySlot[entity].Values)
            {
                foreach (Item item in items)
                {
                    if (includeNested && item.entity.entityType.CanHaveInventory())
                    {
                        results.AddRange(item.entity.inventoryType.GetAllEntities(item.entity, true));
                    }
                    results.Add(item.entity);
                }
            }
            return results;
        }

        public virtual List<Entity> GetAllEntities(Entity entity, EntityType entityType, bool includeNested = false)
        {
            List<Entity> results = GetAllEntities(entity, includeNested);

            return results.Where(x => entityType == x.entityType).ToList();
        }

        public virtual List<Entity> GetAllEntities(Entity entity, List<EntityType> entityTypes, bool includeNested = false)
        {
            List<Entity> results = GetAllEntities(entity, includeNested);

            return results.Where(x => entityTypes.Contains(x.entityType)).ToList();
        }

        public virtual List<EntityType> GetAllEntityTypes(Entity entity, bool includeNested = false)
        {
            if (!includeNested)
                return inventoryByType[entity].Keys.ToList();

            List<Entity> results = GetAllEntities(entity, includeNested);
            return results.Select(x => x.entityType).Distinct().ToList();
        }

        public virtual Entity GetFirstEntityofEntityType(Entity entity, EntityType entityType, bool includeNested = false)
        {
            if (!includeNested)
            {
                if (inventoryByType[entity].TryGetValue(entityType, out List<Item> items))
                    return items[0].entity;
                return null;
            }

            List<Entity> results = GetAllEntities(entity, includeNested);
            return results.Find(x => x.entityType);
        }

        public virtual List<Entity> GetAllEntitiesInSlot(Entity entity, InventorySlot inventorySlot)
        {
            if (inventoryBySlot[entity].TryGetValue(inventorySlot, out List<Item> items) && items.Count > 0)
            {
                return items.Select(x => x.entity).ToList();
            }
            return null;
        }

        public virtual Entity GetFirstEntityInSlot(Entity entity, InventorySlot inventorySlot)
        {
            if (inventoryBySlot[entity].TryGetValue(inventorySlot, out List<Item> items) && items.Count > 0)
            {
                return items[0].entity;
            }
            return null;
        }

        public virtual InventorySlot GetEntitySlot(Entity entity, Entity entityInInventory)
        {
            if (inventoryByType[entity].TryGetValue(entityInInventory.entityType, out List<Item> items))
            {
                Item item = items.Find(x => x.entity == entityInInventory);
                return item?.inventorySlot;
            }
            return null;
        }

        public virtual List<AudioClip> GetItemSoundEffects(Entity entity, ActionType actionType, ActionType otherActionType,
                                                           ItemCondition.ModifierType modifierType, bool isTarget, int tagEnumIndex)
        {
            Agent agent = entity as Agent;
            if (agent == null)
            {
                Debug.LogError(entity.name + ": GetItemSoundEffects called for a Non-Agent Entity.");
                return null;
            }

            List<AudioClip> audioClips = new List<AudioClip>();
            foreach (KeyValuePair<InventorySlot, List<Item>> items in inventoryBySlot[entity])
            {
                foreach (Item item in items.Value)
                {
                    bool includeItem = false;
                    switch (modifierType)
                    {
                        case ItemCondition.ModifierType.Used:
                            if (IsUsed(entity, item, actionType))
                                includeItem = true;
                            break;
                        case ItemCondition.ModifierType.Equipped:
                            if (IsEquipped(entity, item))
                                includeItem = true;
                            break;
                        case ItemCondition.ModifierType.InInventory:
                            includeItem = true;
                            break;
                    }

                    if (includeItem)
                    {
                        foreach (ItemSoundEffects itemSoundEffects in item.entity.entityType.itemSoundEffects)
                        {
                            if (itemSoundEffects.itemCondition.Check(agent, actionType, otherActionType, item.inventorySlot, modifierType, isTarget))
                            {
                                audioClips.AddRange(itemSoundEffects.GetAudioClips(item.entity.prefabVariantIndex, tagEnumIndex));
                            }
                        }
                    }
                }
            }
            return audioClips;
        }

        public virtual List<GameObject> GetItemVisualEffects(Entity entity, ActionType actionType, ActionType otherActionType,
                                                            ItemCondition.ModifierType modifierType, bool isTarget)
        {
            Agent agent = entity as Agent;
            if (agent == null)
            {
                Debug.LogError(entity.name + ": GetItemVisualEffects called for a Non-Agent Entity.");
                return null;
            }

            List<GameObject> gameObjects = new List<GameObject>();
            foreach (KeyValuePair<InventorySlot, List<Item>> items in inventoryBySlot[entity])
            {
                foreach (Item item in items.Value)
                {
                    bool includeItem = false;
                    switch (modifierType)
                    {
                        case ItemCondition.ModifierType.Used:
                            if (IsUsed(entity, item, actionType))
                                includeItem = true;
                            break;
                        case ItemCondition.ModifierType.Equipped:
                            if (IsEquipped(entity, item))
                                includeItem = true;
                            break;
                        case ItemCondition.ModifierType.InInventory:
                            includeItem = true;
                            break;
                    }

                    if (includeItem)
                    {
                        foreach (ItemVisualEffects itemVisualEffects in item.entity.entityType.itemVisualEffects)
                        {
                            if (itemVisualEffects.itemCondition.Check(agent, actionType, otherActionType, item.inventorySlot, modifierType, isTarget))
                            {
                                gameObjects.AddRange(itemVisualEffects.GetGameObjects(item.entity.prefabVariantIndex));
                            }
                        }
                    }
                }
            }
            return gameObjects;
        }

        public virtual float TotalItemActionSkillModifiersValue(Entity entity, ActionType actionType, float level)
        {
            Agent agent = entity as Agent;
            if (agent == null)
            {
                Debug.LogError(entity.name + ": GetTotalActionSkillModifiersValue called for a Non-Agent Entity.");
                return 0f;
            }

            // This ignores nested inventory
            float newLevel = level;
            foreach (KeyValuePair<InventorySlot, List<Item>> items in inventoryBySlot[entity])
            {
                foreach (Item item in items.Value)
                {
                    foreach (ItemActionSkillModifier modifier in item.entity.entityType.itemActionSkillModifiers)
                    {
                        bool includeItem = false;
                        switch (modifier.itemCondition.modifierType)
                        {
                            case ItemCondition.ModifierType.Used:
                                if (IsUsed(entity, item, actionType))
                                    includeItem = true;
                                break;
                            case ItemCondition.ModifierType.Equipped:
                                if (IsEquipped(entity, item))
                                    includeItem = true;
                                break;
                            case ItemCondition.ModifierType.InInventory:
                                includeItem = true;
                                break;
                        }
                        
                        if (includeItem && modifier.Check(agent, item, actionType))
                        {
                            float value = modifier.GetValue(item.entity.prefabVariantIndex);
                            switch (modifier.modifyValueType)
                            {
                                case ItemActionSkillModifier.ModifyValueType.Add:
                                    newLevel += value;
                                    break;
                                case ItemActionSkillModifier.ModifyValueType.Multiply:
                                    newLevel *= value;
                                    break;
                                case ItemActionSkillModifier.ModifyValueType.Override:
                                    newLevel = value;
                                    break;
                                case ItemActionSkillModifier.ModifyValueType.Veto:
                                    return value;
                            }
                        }
                    }

                }
            }
            Debug.Log(agent.name + ": GetTotalActionSkillModifiersValue: AT = " + actionType.name + " - result = " + newLevel);
            return newLevel;
        }

        public virtual float GetAlwaysItemAttributeTypeModifiers(Entity entity, AttributeType attributeType, float level,
                                                                 ItemAttributeTypeModifier.ChangeType changeType)
        {
            Agent agent = entity as Agent;
            if (agent == null)
            {
                Debug.LogError(entity.name + ": GetAlwaysItemAttributeTypeModifiers called for a Non-Agent Entity.");
                return 0f;
            }

            // This ignores nested inventory
            float newLevel = level;
            foreach (KeyValuePair<InventorySlot, List<Item>> items in inventoryBySlot[entity])
            {
                foreach (Item item in items.Value)
                {
                    foreach (ItemAttributeTypeModifier modifier in item.entity.entityType.itemAttributeTypeModifiers
                                                                       .Where(x => x.modifierType == ItemAttributeTypeModifier.ModifierType.Always &&
                                                                                   x.attributeType == attributeType))
                    {
                        bool includeItem = false;
                        switch (modifier.itemCondition.modifierType)
                        {
                            case ItemCondition.ModifierType.Used:
                                Debug.LogError(entity.name + ": GetAlwaysItemAttributeTypeModifiers - Always modifier's Item Condition is of ModifierType " +
                                               "'Used' - Can't be Used for an Always modifier.");
                                break;
                            case ItemCondition.ModifierType.Equipped:
                                if (IsEquipped(entity, item))
                                    includeItem = true;
                                break;
                            case ItemCondition.ModifierType.InInventory:
                                includeItem = true;
                                break;
                        }

                        if (includeItem && modifier.itemCondition.Check(agent, null, null, item.inventorySlot, 0, false, true, true))
                        {
                            float value = modifier.GetValue(item.entity.prefabVariantIndex, changeType);
                            if (value != float.PositiveInfinity)
                            {
                                switch (modifier.modifyValueType)
                                {
                                    case ItemAttributeTypeModifier.ModifyValueType.Add:
                                        newLevel += value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Multiply:
                                        newLevel *= value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Override:
                                        newLevel = value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Veto:
                                        return value;
                                }
                            }
                        }
                    }
                }
            }

            //Debug.Log(agent.name + ": GetAlwaysItemAttributeTypeModifiers: AtributeType = " + attributeType.name + " - result = " + newLevel);
            return newLevel;
        }

        public virtual float GetItemAttributeTypeModifiersValue(Entity entity, AttributeType attributeType, ActionType actionType,
                                                                ActionType otherActionType, bool isTarget)
        {
            Agent agent = entity as Agent;
            if (agent == null)
            {
                Debug.LogError(entity.name + ": GetItemAttributeTypeModifiersValue called for a Non-Agent Entity.");
                return 0f;
            }

            float result = 0f;
            foreach (KeyValuePair<InventorySlot, List<Item>> items in inventoryBySlot[entity])
            {
                foreach (Item item in items.Value)
                {
                    // See if this item has any Item Attribute Type Modifiers that pass all conditions
                    foreach (ItemAttributeTypeModifier modifier in item.entity.entityType.itemAttributeTypeModifiers
                                                                       .Where(x => x.modifierType == ItemAttributeTypeModifier.ModifierType.OnAction))
                    {
                        switch (modifier.itemCondition.modifierType)
                        {
                            case ItemCondition.ModifierType.Used:
                                if (!IsUsed(entity, item, actionType))
                                    continue;
                                break;
                            case ItemCondition.ModifierType.Equipped:
                                if (!IsEquipped(entity, item))
                                    continue;
                                break;
                        }

                        // Just checked the Modifier Type so skip checking it again
                        if (modifier.itemCondition.Check(agent, actionType, otherActionType, item.inventorySlot, 0, isTarget, false, true))
                        {
                            float value = modifier.GetValue(item.entity.prefabVariantIndex, ItemAttributeTypeModifier.ChangeType.Level);
                            if (value != float.PositiveInfinity)
                            {
                                switch (modifier.modifyValueType)
                                {
                                    case ItemAttributeTypeModifier.ModifyValueType.Add:
                                        result += value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Multiply:
                                        result *= value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Override:
                                        result = value;
                                        break;
                                    case ItemAttributeTypeModifier.ModifyValueType.Veto:
                                        return value;
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log(agent.name + ": GetItemAttributeTypeModifiersValue: AT = " + actionType.name + " OtherAT = " +
                      (otherActionType == null ? "Null" : otherActionType.name) + " - result = " + result);
            return result;
        }

        public virtual float CalcOnActionAttributeTypeModifiers(Agent agent, AttributeType attributeType, ActionType actionType, ActionType otherActionType,
                                                                Entity entityInInventory, ItemCondition.ModifierType modifierType, bool isTarget)
        {
            Item item = GetInventoryItem(agent, entityInInventory);
            return CalcOnActionAttributeTypeModifiers(agent, attributeType, actionType, otherActionType, item, modifierType, isTarget);
        }

        // Returns the ActionTypeModifier Value for one Item for the specified actionType
        public virtual float CalcOnActionAttributeTypeModifiers(Agent agent, AttributeType attributeType, ActionType actionType, ActionType otherActionType,
                                                                Item item, ItemCondition.ModifierType modifierType, bool isTarget)
        {
            float result = 0f;
            foreach (ItemAttributeTypeModifier modifier in item.entity.entityType.itemAttributeTypeModifiers
                                                               .Where(x => x.modifierType == ItemAttributeTypeModifier.ModifierType.OnAction &&
                                                                           x.attributeType == attributeType))
            {
                if (modifier.itemCondition.Check(agent, actionType, otherActionType, item.inventorySlot, modifierType, isTarget))
                {
                    float value = modifier.GetValue(item.entity.prefabVariantIndex, ItemAttributeTypeModifier.ChangeType.Level);
                    if (value != float.PositiveInfinity)
                    {
                        switch (modifier.modifyValueType)
                        {
                            case ItemAttributeTypeModifier.ModifyValueType.Add:
                                result += value;
                                break;
                            case ItemAttributeTypeModifier.ModifyValueType.Multiply:
                                result *= value;
                                break;
                            case ItemAttributeTypeModifier.ModifyValueType.Override:
                                result = value;
                                break;
                            case ItemAttributeTypeModifier.ModifyValueType.Veto:
                                return value;
                        }
                    }
                }
            }
            Debug.Log(agent.name + ": CalcActionTypeModifiers: AT = " + actionType.name + " OtherAT = " +
                      (otherActionType == null ? "Null" : otherActionType.name) + " - result = " + result);
            return result;
        }

        public virtual bool IsEquipped(Entity entity, Entity inventoryEntity)
        {
            Item item = GetInventoryItem(entity, inventoryEntity);
            return IsEquipped(entity, item);
        }

        public virtual bool IsEquipped(Entity entity, Item item)
        {
            return item != null && item.entity.entityType.canBeInInventorySlots.Any(x => x.inventorySlot == item.inventorySlot && x.isEquipped);
        }

        public virtual bool IsEquipped(Agent agent, EntityType entityType)
        {
            foreach (Item item in inventoryByType[agent][entityType])
            {
                if (IsEquipped(agent, item))
                    return true;
            }
            return false;
        }

        public virtual List<InventorySlot> EquippedSlots(Entity entity, EntityType entityType)
        {
            return entityType.canBeInInventorySlots.FindAll(x => x.isEquipped).Select(x => x.inventorySlot).ToList();
        }

        public virtual bool IsUsed(Entity entity, Item item, ActionType actionType)
        {
            return IsEquipped(entity, item) && actionType.usesInventorySlots.Contains(item.inventorySlot);
        }

        public virtual void MoveAllInventoryTo(Entity fromEntity, Entity toEntity)
        {
            List<Entity> allInventoryEntities = GetAllEntities(fromEntity);
            foreach (Entity entity in allInventoryEntities)
            {
                fromEntity.inventoryType.Remove(fromEntity, entity);
                toEntity.inventoryType.Add(toEntity, entity);
            }
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

        public virtual string InfoForGUI(Entity entity)
        {
            string info = "";
            if (inventoryByType[entity].Count > 0)
                info += "\n<b>Inventory</b>\n";

            foreach (KeyValuePair<EntityType, List<Item>> entityTypes in inventoryByType[entity])
            {
                foreach (Item item in entityTypes.Value)
                {
                    info += entityTypes.Key.name + ": " + item.entity.name + " in " + item.inventorySlot.name + "\n";
                }
            }
            return info;
        }
    }
}
