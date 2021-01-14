using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public delegate void EntityDisabled(Entity entity);
    public delegate void EntityDestroyed(Entity entity);

    public abstract class Entity : MonoBehaviour
    {
        [Serializable]
        public class DefaultTagWithEntity : EntityType.DefaultTag
        {
            public Entity entity;
        }
        public List<DefaultTagWithEntity> defaultTagsWithEntity;

        public EntityType entityType;
        public List<EntityTypeOverride> entityTypeOverrides;

        public int prefabVariantIndex;

        public Dictionary<AttributeType, Attribute> attributes;
        public Dictionary<TagType, List<Tag>> tags;

        [Serializable]
        public class InventorySlotMapping
        {
            public InventorySlot inventorySlot;
            public List<GameObject> locations;
            // TODO: How does skinning work?
        }
        public List<InventorySlotMapping> inventorySlotMappings;

        [HideInInspector]
        public InventoryType inventoryType;

        public Entity inEntityInventory;

        private List<EntityTrigger> entityTriggers;

        public Vector3 goingToLocation;
        
        public event EntityDisabled EntityDisabledEvent;
        public event EntityDestroyed EntityDestroyedEvent;

        [HideInInspector]
        public TotalAIManager totalAIManager;

        protected int entityLayers;

        public abstract void DestroySelf(Agent agent, float delay = 0f);

        public void Awake()
        {
            if (entityType == null)
            {
                Debug.LogError(name + ": Missing Entity Type.  Please fix.");
                return;
            }
            entityLayers = LayerMask.GetMask("Agent", "WorldObject", "Area", "AgentEvent");
            
            // TODO: Get rid of inside area stuff?
            //AddAllInsideAreas();

            GameObject managerGameObject = GameObject.Find("TotalAIManager");
            if (managerGameObject == null)
            {
                Debug.LogError("Please add an Empty GameObject named TotalAIManager to the scene and add TotalAIManager component to it.");
                return;
            }
            totalAIManager = managerGameObject.GetComponent<TotalAIManager>();
            if (totalAIManager == null)
            {
                Debug.LogError("Please add TotalAIManager component to the TotalAIManager GameObject.");
                return;
            }
        }

        public void ResetEntity(bool skipCreatingEntities = false)
        {
            ResetAttributes();
            ResetTags();
            ResetEntityTriggers();

            // TODO: Turn into method
            inventoryType = entityType.defaultInventoryType;
            if (inventoryType == null)
            {
                Debug.LogError(name + ": EntityType = " + entityType.name + " is missing a defaultInventoryType.  Please fix.");
                return;
            }
            inventoryType.SetupEntity(this);
            if (!skipCreatingEntities)
                inventoryType.Reset(this);

            if (entityType.HasAnyInteractionSpotTypes())
                entityType.SetupInteractionSpotTypes(this);
        }

        public void OnEnable()
        {
            totalAIManager.EntityEnabled(this);
        }

        public void OnDisable()
        {
            Debug.Log(gameObject.name + ": Disabled");

            EntityDisabledEvent?.Invoke(this);
        }

        public virtual void OnDestroy()
        {
            Debug.Log(gameObject.name + ": Destroyed");

            EntityDestroyedEvent?.Invoke(this);
        }

        private void ResetAttributes()
        {
            attributes = new Dictionary<AttributeType, Attribute>();
            foreach (EntityType.DefaultAttribute defaultAttribute in entityType.defaultAttributes)
            {
                if (defaultAttribute.type is OneFloatAT)
                    attributes.Add(defaultAttribute.type, new Attribute(this, defaultAttribute.type, defaultAttribute.oneFloatData));
                else if (defaultAttribute.type is MinMaxFloatAT)
                    attributes.Add(defaultAttribute.type, new Attribute(this, defaultAttribute.type, defaultAttribute.minMaxFloatData));
                else if (defaultAttribute.type is EnumAT)
                    attributes.Add(defaultAttribute.type, new Attribute(this, defaultAttribute.type, defaultAttribute.enumData));
            }

            if (entityTypeOverrides != null)
            {
                foreach (EntityTypeOverride entityTypeOverride in entityTypeOverrides)
                {
                    if (entityTypeOverride != null)
                    {
                        foreach (EntityTypeOverride.OverrideAttribute overrideAttribute in entityTypeOverride.overrideAttributes)
                        {
                            if (overrideAttribute.overrideType == EntityOverrideType.AddOrReplace)
                            {
                                // attributes[overrideAttribute.type] = new Attribute(this, overrideAttribute.type, overrideAttribute.data);
                            }
                            else
                            {
                                attributes.Remove(overrideAttribute.type);
                            }
                        }
                    }
                }
            }
        }

        private void ResetEntityTriggers()
        {
            entityTriggers = new List<EntityTrigger>();
            foreach (EntityTrigger entityTrigger in entityType.defaultEntityTriggers)
            {
                entityTriggers.Add(entityTrigger);
            }
        }

        private void ResetTags()
        {
            tags = new Dictionary<TagType, List<Tag>>();

            foreach (EntityType.DefaultTag tagTypeInfo in entityType.defaultTags)
            {
                AddTag(tagTypeInfo.type, null, tagTypeInfo.level);
            }

            // TODO: Add in overrides

            if (defaultTagsWithEntity != null)
            {
                foreach (DefaultTagWithEntity defaultTag in defaultTagsWithEntity)
                {
                    AddTag(defaultTag.type, defaultTag.entity, defaultTag.level);
                }
            }
        }

        public void AddTag(TagType tagType, Entity entity, float level = 0)
        {
            if (tagType == null)
                return;

            Tag newTag = new Tag(tagType, entity, level);

            if (tags.TryGetValue(tagType, out List<Tag> existingTags))
            {
                //TODO: Should this check to see if this tag/owner combo already exists?
                existingTags.Add(newTag);
            }
            else
            {
                tags.Add(tagType, new List<Tag>() { newTag });
            }
        }

        public void RemoveTag(TagType tagType, Agent agent)
        {
            if (agent == null)
            {
                tags.Remove(tagType);
            }
            else if (tags.TryGetValue(tagType, out List<Tag> existingTags))
            {
                Tag tagToRemove = existingTags.Find(x => x.relatedEntity == agent);
                if (tagToRemove != null)
                    existingTags.Remove(tagToRemove);
            }
        }

        public bool HasTag(TagType tagType)
        {
            return tags.ContainsKey(tagType);
        }

        public GameObject GetSlotMapping(InventorySlot inventorySlot, out bool makeDisabled, bool alreadyAdded = false)
        {
            makeDisabled = false;
            InventorySlotMapping inventorySlotMapping = inventorySlotMappings.Find(x => x.inventorySlot == inventorySlot);
            if (inventorySlotMapping == null)
            {
                Debug.LogError(name + ": Trying to put an Entity in an InventorySlot (" + inventorySlot + ") Location that does " +
                               "not have an InventorySlotMapping. (On the Entity Component - please set the InventorySlotMapping variable)");
                return null;
            }

            int numInSlot = inventoryType.GetInventorySlotAmount(this, inventorySlot);
            if (alreadyAdded)
                --numInSlot;

            int locationIndex = -1;
            switch (inventorySlot.slotType)
            {
                case InventorySlot.SlotType.Invisible:
                case InventorySlot.SlotType.OwnerInvisible:
                    locationIndex = 0;
                    break;
                case InventorySlot.SlotType.Location:
                    if (numInSlot > 1)
                    {
                        Debug.LogError(name + ": Entity.GetSlotMapping failed to find slot location in InventorySlot = " + inventorySlot.name);
                        return null;
                    }
                    locationIndex = 0;
                    break;
                case InventorySlot.SlotType.MultiLocation:
                    locationIndex = numInSlot;
                    if (numInSlot >= inventorySlotMapping.locations.Count)
                    {
                        if (inventorySlot.extraBecomesInvisible)
                        {
                            // Place item in the last slot and tell caller it should be disabled
                            makeDisabled = true;
                            locationIndex = inventorySlotMapping.locations.Count - 1;
                        }
                        else
                        {
                            Debug.LogError(name + ": Entity.GetSlotMapping failed to find slot location in InventorySlot = " + inventorySlot.name);
                            return null;
                        }
                    }
                    break;
                case InventorySlot.SlotType.Skinned:
                    Debug.LogError("InventorySlot (" + inventorySlot.name + ") - Skinned Slot type is currently not supported.");
                    return null;
            }

            if (locationIndex >= inventorySlotMapping.locations.Count)
            {
                Debug.LogError(name + ": GetSlotMapping (" + inventorySlot.name + ") tried to use a inventorySlotMapping.locations " + 
                               "that doesn't exist: " + locationIndex + " >= " + inventorySlotMapping.locations.Count);
                return null;
            }

            return inventorySlotMapping.locations[locationIndex];
        }

        public GameObject PrefabVariant()
        {
            // If the prefab variants are not set up - just use the GameObject its attached to
            if (prefabVariantIndex < 0 || prefabVariantIndex >= entityType.prefabVariants.Count)
            {
                Debug.LogError(name + ": Trying to access the Prefab Variant for " + entityType.name + " at index = " +
                               prefabVariantIndex + ".  It does not exist.  Please fix.");
                return null;
            }
            return entityType.prefabVariants[prefabVariantIndex];
        }

        public GameObject GetVFXGameObject(int index)
        {
            string transformName = "VFXSpot";
            if (index >= 0)
                transformName += index.ToString();
            else
                return gameObject;

            GameObject result = GetFXGameObject(gameObject, transformName);
            if (result == null)
                return gameObject;
            return result;
        }

        public GameObject GetSFXGameObject(int index)
        {
            string transformName = "SFXSpot";
            if (index >= 0)
                transformName += index.ToString();
            else
                return gameObject;

            GameObject result = GetFXGameObject(gameObject, transformName);
            if (result == null)
                return gameObject;
            return result;
        }

        private static GameObject GetFXGameObject(GameObject gameObject, string transformName)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.name == transformName)
                    return child.gameObject;

                if (child.childCount > 0)
                {
                    GameObject result = GetFXGameObject(child.gameObject, transformName);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        // TODO: Just handles moving to empty slot - need to handle swapping entities
        public void SwitchInventorySlots(InventoryType.Item itemToMove, InventorySlot newSlot)
        {
            GameObject gameObjectToMove = itemToMove.entity.gameObject;

            gameObjectToMove.transform.parent = GetSlotMapping(newSlot, out bool makeDisabled).transform;
            gameObjectToMove.transform.localPosition = Vector3.zero;

            if (newSlot.slotType != InventorySlot.SlotType.Invisible && !makeDisabled)
                gameObjectToMove.SetActive(true);
            else
                gameObjectToMove.SetActive(false);
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
            Entity target = collision.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                Debug.Log(name + ": On Collision Enter between " + name + " and " + target.name);
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnCollisionEnter);
            }
        }

        public void OnCollisionExit(Collision collision)
        {
            Entity target = collision.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                Debug.Log(name + ": On Collision Exit between " + name + " and " + target.name);
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnCollisionExit);
            }
        }

        public void OnCollisionStay(Collision collision)
        {
            Entity target = collision.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                Debug.Log(name + ": On Collision Stay between " + name + " and " + target.name);
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnCollisionStay);
            }
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            Entity target = other.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                //Debug.Log(name + ": OnTriggerEnter - " + target.name + " Entered the Trigger");
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnTriggerEnter);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            Entity target = other.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                //Debug.Log(name + ": OnTriggerExit - " + target.name + " Exited the Trigger");
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnTriggerExit);
            }
        }

        public void OnTriggerStay(Collider other)
        {
            Entity target = other.gameObject.GetComponent<Entity>();
            if (target != null)
            {
                Debug.Log(name + ": OnTriggerStay - " + target.name + " Is in the Trigger");
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnTriggerStay);
            }
        }

        public void OnParticleCollision(GameObject other)
        {
            Debug.Log(name + ": OnParticleCollision - " + other.name + " hit this Entity.");
            Entity target = other.gameObject.GetComponentInParent<Entity>();
            if (target != null)
            {
                Debug.Log(name + ": OnParticleCollision - " + target.name + " hit this Entity.");
                RunEntityTriggers(target, EntityTrigger.TriggerType.OnParticleCollision);
            }
        }

        public void RunEntityTriggers(Entity target, EntityTrigger.TriggerType triggerType)
        {
            //Debug.Log(name + ": RunEntityModifiers for " + triggerType + " - target = " + (target != null ? target.name : "None"));
            foreach (EntityTrigger entityTrigger in entityTriggers)
            {
                if (entityTrigger != null && entityTrigger.triggerType == triggerType)
                {
                    //Debug.Log(name + ": TryToRun EntityModifier for " + triggerType + " - target = " + (target != null ? target.name : "None"));
                    if (entityTrigger.forTarget && target != null)
                        entityTrigger.TryToRun(target, this);
                    else if (!entityTrigger.forTarget)
                        entityTrigger.TryToRun(this, target);
                }
            }
        }

        public bool HasRequiredInventorySlotMappings()
        {
            if (entityType != null)
            {
                if ((entityType.inventorySlots == null || entityType.inventorySlots.Count == 0) &&
                    (inventorySlotMappings == null || inventorySlotMappings.Count == 0))
                    return true;
                else if (entityType.inventorySlots == null || inventorySlotMappings == null ||
                         entityType.inventorySlots.Count != inventorySlotMappings.Count)
                    return false;

                // Lengths are same - make sure the inventorySlots match
                foreach (InventorySlotMapping inventorySlotMapping in inventorySlotMappings)
                {
                    if (!entityType.inventorySlots.Contains(inventorySlotMapping.inventorySlot))
                        return false;
                }
            }
            return true;
        }

        public void OnDrawGizmosSelected()
        {
            if (entityType != null)
            {
                Gizmos.color = new Color(1, 1, 0, 0.75F);
                Gizmos.DrawWireSphere(transform.position, entityType.maxDistanceAsInput);

                if (goingToLocation != Vector3.zero)
                    Gizmos.DrawWireSphere(goingToLocation, .25f);
            }
        }
    }
}