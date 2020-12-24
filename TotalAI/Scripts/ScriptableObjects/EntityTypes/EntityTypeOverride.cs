using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public enum EntityOverrideType { AddOrReplace, Remove }

    public class EntityTypeOverride : ScriptableObject
    {
        [Serializable]
        public class OverrideAttribute
        {
            public EntityOverrideType overrideType;
            public AttributeType type;
            public AttributeType.Data data;
        }
        [Header("Override Attributes")]
        public List<OverrideAttribute> overrideAttributes;

        [Serializable]
        public class OverrideTag
        {
            public EntityOverrideType overrideType;
            public TagType type;
            public Agent owner;
            public float level;
        }
        [Header("Override Tags")]
        public List<OverrideTag> overrideTags;

        [Serializable]
        public class OverrideInventoryItem
        {
            public EntityOverrideType overrideType;
            public EntityType type;
            public int prefabVariantIndex;
            public InventorySlot inventorySlot;
            [Range(1, 100)]
            public int numberOfTypes = 1;
            [Tooltip("Chance that Entity spawns with this Inventory EntityType.  Set to 100 for it to always spawn.")]
            [Range(0, 100)]
            public float probability = 100;
        }
        [Header("Override Inventory")]
        public List<OverrideInventoryItem> overrideInventory;
    }
}