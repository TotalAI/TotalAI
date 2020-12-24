using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public abstract class EntityType : InputOutputType
    {
        [Tooltip("Does this EntityType have custom interaction spot logic?  Leave None to use the MovementType's default interaction spot logic.")]
        public InteractionSpotType interactionSpotType;

        [Serializable]
        public class SpotTypeActionTypeMapping
        {
            public InteractionSpotType interactionSpotType;
            public List<ActionType> actionTypes;
        }
        [Header("Specific Action Types can have different Interaction Spot Types")]
        public List<SpotTypeActionTypeMapping> spotTypeActionTypeMappings;

        public List<GameObject> prefabVariants;

        [Header("Order matches the Prefab Variants Order")]
        public List<GameObject> ghostPrefabs;
        public bool requiresFlatLand;           // Entity can only be placed on a flat location on terrain
        public bool rotateToTerrain;            // Entity will be placed to fit the slope of the terrain
        public float maxSlopePlacement;         // Max slope that an object can be placed on

        public float maxDistanceAsInput;        // The default distance to this entity type for it to be used as an input in a mapping

        [Serializable]
        public class DefaultAttribute
        {
            public AttributeType type;

            // TODO: Fix this and in Entity when it gets set
            //       Need someway to set the correct Data type when this is created by the inspector - in ReordableList Add Callback
            public OneFloatAT.OneFloatData oneFloatData;
            public MinMaxFloatAT.MinMaxFloatData minMaxFloatData;
            public EnumAT.EnumData enumData;
        }
        public List<DefaultAttribute> defaultAttributes;

        [Serializable]
        public class DefaultTag
        {
            public TagType type;
            public float level;
        }
        public List<DefaultTag> defaultTags;

        public InventoryType defaultInventoryType;

        [Serializable]
        public class CanBeInInventorySlot
        {
            public InventorySlot inventorySlot;
            public int numLocations = 1;
            [Tooltip("When the EntityType is in this slot is it consided to be equipped?")]
            public bool isEquipped;
            public List<InventorySlot> otherInventorySlots;
            public List<int> otherInventorySlotsNumLocations;
        }
        public List<CanBeInInventorySlot> canBeInInventorySlots;

        [Serializable]
        public class TransformMapping
        {
            public GameObject transform;
            public InventorySlot inventorySlot;
            public InputCondition.MatchType ownerMatchType;
            public TypeGroup typeGroupMatch;
            public TypeCategory typeCategoryMatch;
            public EntityType entityTypeMatch;
            public int ownerPrefabVariantIndex = -1;
            public int thisPrefabVariantIndex = -1;
            public List<string> stateNames;
        }
        public List<TransformMapping> transformMappings;

        public List<ItemAttributeTypeModifier> itemAttributeTypeModifiers;
        public List<ItemActionSkillModifier> itemActionSkillModifiers;
        public List<ItemSoundEffects> itemSoundEffects;
        public List<ItemVisualEffects> itemVisualEffects;

        public List<InventorySlot> inventorySlots;
        
        [Serializable]
        public class DefaultInventory
        {
            public InventorySlot inventorySlot;
            public EntityType entityType;
            public int prefabVariantIndex;
            public float probability = 100;
            public MinMaxCurve amountCurve;
        }
        public List<DefaultInventory> defaultInventory;

        public List<EntityModifier> defaultEntityModifiers;

        public abstract GameObject CreateEntity(int prefabVariantIndex, Vector3 position, Quaternion rotation, Vector3 scale, Entity creator);

        public abstract Entity CreateEntityInInventory(Entity entity, int prefabVariantIndex, InventorySlot inventorySlot);

        public bool HasAnyInteractionSpotTypes()
        {
            return interactionSpotType != null || (spotTypeActionTypeMappings != null && spotTypeActionTypeMappings.Count > 0);
        }

        public void SetupInteractionSpotTypes(Entity entity)
        {
            List<InteractionSpotType> interactionSpotTypes = new List<InteractionSpotType>();

            if (interactionSpotType != null)
                interactionSpotTypes.Add(interactionSpotType);

            if (spotTypeActionTypeMappings != null)
            {
                foreach (SpotTypeActionTypeMapping spotTypeActionTypeMapping in spotTypeActionTypeMappings)
                {
                    if (!interactionSpotTypes.Contains(spotTypeActionTypeMapping.interactionSpotType))
                        interactionSpotTypes.Add(spotTypeActionTypeMapping.interactionSpotType);
                }
            }

            foreach (InteractionSpotType interactionSpotType in interactionSpotTypes)
            {
                interactionSpotType.Setup(entity);
            }
        }

        public InteractionSpotType GetInteractionSpotType(Mapping mapping)
        {
            ActionType actionType = mapping.NonGoToActionType();
            foreach (SpotTypeActionTypeMapping spotTypeActionTypeMapping in spotTypeActionTypeMappings)
            {
                if (spotTypeActionTypeMapping.actionTypes.Contains(actionType))
                    return spotTypeActionTypeMapping.interactionSpotType;
            }
            return interactionSpotType;
        }

        // Move to InventoryType?  Along with ones below?
        public GameObject GetTransformMapping(InventorySlot inventorySlot, Entity inventoryOwner, Entity entityBecomingInventory)
        {
            if (inventorySlot == null || !canBeInInventorySlots.Any(x => x.inventorySlot == inventorySlot))
            {
                Debug.Log(name + ": GetTransformMapping - canBeInInventorySlots is missing an InventorySlot - " +
                          (inventorySlot == null ? "Null" : inventorySlot.name) + " - using prefab variant");
                return entityBecomingInventory.PrefabVariant();
            }
            
            List<TransformMapping> mappings = transformMappings.FindAll(x => x.inventorySlot == inventorySlot &&
                                                                             TransformMappingMatch(x, inventoryOwner, entityBecomingInventory));
            if (mappings.Count == 0)
            {
                Debug.Log(name + ": GetTransformMapping - transformMappings is missing an InventorySlot/EntityType combo - " +
                          inventorySlot.name + "/" + inventoryOwner.name + "- using prefab variant");
                return entityBecomingInventory.PrefabVariant();
            }
            if (mappings.Count == 1)
                return mappings[0].transform;

            return PickTransformMapping(mappings, entityBecomingInventory);
        }

        // TODO: Move this?
        // Checks MatchType and PrefabVariantIndex
        private bool TransformMappingMatch(TransformMapping transformMapping, Entity inventoryOwner, Entity entityBecomingInventory)
        {
            // TODO: Throw error if indexes are invalid
            if (transformMapping.thisPrefabVariantIndex >= 0 &&
                transformMapping.thisPrefabVariantIndex < prefabVariants.Count &&
                entityBecomingInventory.prefabVariantIndex != transformMapping.thisPrefabVariantIndex)
                return false;

            if (transformMapping.ownerPrefabVariantIndex >= 0 &&
                transformMapping.ownerPrefabVariantIndex < inventoryOwner.entityType.prefabVariants.Count &&
                inventoryOwner.prefabVariantIndex != transformMapping.ownerPrefabVariantIndex)
                return false;

            switch (transformMapping.ownerMatchType)
            {
                case InputCondition.MatchType.TypeGroup:
                    if (transformMapping.typeGroupMatch == null ||
                        !transformMapping.typeGroupMatch.InGroup(inventoryOwner.entityType))
                        return false;
                    break;
                case InputCondition.MatchType.TypeCategory:
                    if (transformMapping.typeCategoryMatch == null ||
                        !transformMapping.typeCategoryMatch.IsCategoryOrDescendantOf(inventoryOwner.entityType))
                        return false;
                    break;
                case InputCondition.MatchType.EntityType:
                    if (transformMapping.entityTypeMatch == null ||
                        transformMapping.entityTypeMatch != inventoryOwner.entityType)
                        return false;
                    break;
            }

            return true;
        }

        private GameObject PickTransformMapping(List<TransformMapping> mappings, Entity entityBecomingInventory)
        {
            // Only WorldObjects have states
            WorldObject worldObject = entityBecomingInventory as WorldObject;
            if (worldObject == null || worldObject.currentState == null)
                return mappings[0].transform;

            // Use first one that matches the states
            List<TransformMapping> matches = mappings.FindAll(x => x.stateNames.Contains(worldObject.currentState.name));
            if (matches.Count > 0)
                return matches[0].transform;

            // No state matches - see if there is one with no states
            matches = mappings.FindAll(x => x.stateNames == null || x.stateNames.Count == 0);
            if (matches.Count > 0)
                return matches[0].transform;

            Debug.Log(name + ": PickTransformMapping was unable to find a states match -  using prefab variant");
            return entityBecomingInventory.PrefabVariant();
        }

        public bool CanHaveInventory()
        {
            return inventorySlots != null && inventorySlots.Count > 0;
        }

        public bool CanBeInInventory()
        {
            return canBeInInventorySlots != null && canBeInInventorySlots.Count > 0;
        }

        public bool InAllTypeGroups(List<TypeGroup> typeGroups)
        {
            foreach (TypeGroup typeGroup in typeGroups)
            {
                if (!typeGroup.InGroup(this))
                    return false;
            }
            return true;
        }

        // TODO: Move this to a static utility class
        public static int GetLayer(Entity entity, bool ignoreInventory = false)
        {
            EntityType entityType = entity.entityType;

            if (!ignoreInventory && entity.inEntityInventory != null)
            {
                return LayerMask.NameToLayer("Inventory");
            }
            else if (entityType is AgentType)
            {
                return LayerMask.NameToLayer("Agent");
            }
            else if (entityType is WorldObjectType)
            {
                return LayerMask.NameToLayer("WorldObject");
            }
            else if (entityType is AgentEventType)
            {
                return LayerMask.NameToLayer("AgentEvent");
            }
            return 0;
        }

        // TODO: Move this to a static utility class
        public static Type GetEntityType(Entity entity)
        {
            if (entity is Agent)
            {
                return typeof(AgentType);
            }
            else if (entity is WorldObject)
            {
                return typeof(WorldObjectType);
            }
            else if (entity is AgentEvent)
            {
                return typeof(AgentEventType);
            }
            return null;
        }
    }
}
