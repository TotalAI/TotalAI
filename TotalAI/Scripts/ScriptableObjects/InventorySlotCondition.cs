using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventorySlotCondition", menuName = "Total AI/Inventory Slot Condition", order = 1)]
    public class InventorySlotCondition : ScriptableObject
    {
        [Tooltip("Leave at None to match all inventory slots.")]
        public InventorySlot inventorySlot;
        public InputCondition.MatchType matchType;
        public EntityType entityType;
        public TypeCategory typeCategory;
        public TypeGroup typeGroup;
        [Tooltip("ONLY applies to entityType match.  Leave at -1 to ignore the Prefab Variant index match.")]
        public int prefabVariantIndex = -1;
        public float minAmount;
        public float maxAmount;

        public bool Check(WorldObject worldObject)
        {
            List<Entity> entities;
            if (inventorySlot != null)
            {
                entities = worldObject.inventoryType.GetAllEntitiesInSlot(worldObject, inventorySlot);
            }
            else
            {
                entities = worldObject.inventoryType.GetAllEntities(worldObject, true);
            }
            if (entities == null || entities.Count == 0)
                return false;

            float amount = 0f;
            switch (matchType)
            {
                case InputCondition.MatchType.TypeGroup:
                    if (!typeGroup.AnyMatches(entities))
                        return false;
                    amount = worldObject.inventoryType.GetTypeGroupAmount(worldObject, typeGroup, inventorySlot);
                    break;
                case InputCondition.MatchType.TypeCategory:
                    if (!typeCategory.AnyInTypeCategoryOrDescendantOf(entities))
                        return false;
                    amount = worldObject.inventoryType.GetTypeCategoryAmount(worldObject, typeCategory, inventorySlot);
                    break;
                case InputCondition.MatchType.EntityType:
                    if (!entities.Exists(x => x.entityType == entityType))
                        return false;
                    amount = worldObject.inventoryType.GetEntityTypeAmount(worldObject, entityType, inventorySlot, prefabVariantIndex);
                    break;
            }

            if (amount < minAmount || amount > maxAmount)
                return false;
            return true;
        }
    }
}
