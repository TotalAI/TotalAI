using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventorySlot", menuName = "Total AI/Inventory Slot", order = 1)]
    public class InventorySlot : ScriptableObject
    {
        public enum SlotType { Invisible, Location, MultiLocation, Skinned, OwnerInvisible }
        [Tooltip("OwnerInvisible will cause the inventory owner to become invisible when slot is occupied by at least one item.")]
        public SlotType slotType;
        [Tooltip("For visible slots once all locations are taken allow extra ones to be invisible")]
        public bool extraBecomesInvisible;
        public enum DropType { InFront, RandomRadius }
        public DropType dropType;
        public float dropDistance = 0.5f;
        [Tooltip("Will allow ones in list and all of their descendants.")]
        public List<TypeCategory> allowedTypeCategories;
        [Tooltip("Set to -1 to ignore max")]
        public int maxNumberEntities = -1;

        public bool EntityTypeAllowed(EntityType entityType)
        {
            if (entityType != null && entityType.typeCategories != null)
            {
                foreach (TypeCategory entityTypeCategory in entityType.typeCategories)
                {
                    foreach (TypeCategory allowedCategory in allowedTypeCategories)
                    {
                        if (entityTypeCategory.IsCategoryOrAncestorOf(allowedCategory))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
