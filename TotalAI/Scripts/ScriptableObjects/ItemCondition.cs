using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ItemCondition", menuName = "Total AI/Item Condition", order = 1)]
    public class ItemCondition : ScriptableObject
    {
        public enum ModifierType { Used, Equipped, InInventory }
        [Tooltip("Does the inventory item need to be used by the ActionType, equipped, or just in inventory?")]
        public ModifierType modifierType;
        [Tooltip("Is the Agent with this inventory the target of the ActionType(s)?")]
        public bool isTarget;
        public List<ActionType> actionTypes;
        public List<ActionType> otherActionTypes;
        public List<InventorySlot> inventorySlots;

        [Serializable]
        public class AgentTypeConstraint
        {
            public AgentType agentType;
            public List<int> prefabVariantsIndexes;
        }
        public List<AgentTypeConstraint> agentTypeConstraints;

        public bool Check(Agent agent, ActionType actionType, ActionType otherActionType, InventorySlot inventorySlot,
                          ModifierType modifierType, bool isTarget, bool allowNoActionTypes = false, bool ignoreModifierType = false)
        {
            if (!allowNoActionTypes && (actionTypes == null || actionTypes.Count == 0) && (otherActionTypes == null || otherActionTypes.Count == 0))
            {
                Debug.LogError(agent.name + ": has an ActionTypeModifier with no Action Types and no Other Action Types.  Please Fix.");
                return false;
            }

            return isTarget == this.isTarget &&
                   (ignoreModifierType || modifierType == this.modifierType) &&
                   (actionTypes == null || actionTypes.Count == 0 || actionTypes.Contains(actionType)) &&
                   (otherActionTypes == null || otherActionTypes.Count == 0 || otherActionTypes.Contains(otherActionType)) &&
                   (inventorySlots != null || inventorySlots.Count == 0 || inventorySlots.Contains(inventorySlot)) &&
                   (agentTypeConstraints != null || agentTypeConstraints.Count == 0 || AgentTypeConstraintsCheck(agent));
        }

        private bool AgentTypeConstraintsCheck(Agent agent)
        {
            return agentTypeConstraints.Any(x => x.agentType == agent.agentType &&
                                            (x.prefabVariantsIndexes == null || x.prefabVariantsIndexes.Count == 0 ||
                                             x.prefabVariantsIndexes.Contains(agent.prefabVariantIndex)));
        }
    }
}
