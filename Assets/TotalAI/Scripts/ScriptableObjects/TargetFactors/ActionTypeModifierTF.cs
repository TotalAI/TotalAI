using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ActionTypeModifierTF", menuName = "Total AI/Target Factors/ActionType Modifier", order = 0)]
    public class ActionTypeModifierTF : TargetFactor
    {
        [Header("For the modifier to apply where does the item need to be?")]
        [Tooltip("These cascade.  For example, An item being Used is also Equipped and InInventory.")]
        public ItemCondition.ModifierType modifierType;

        [Header("What Attribute Type to use for the modifiers?")]
        public AttributeType attributeType;

        [Header("Will the Agent be a target of the Action Type?")]
        public bool isTarget;

        // Evaluates based on the current ActionType modifiers from each Entity
        // For example: MeleeAttack can use this to pick the weapon with the largest modifier
        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            ActionType actionType = mapping.mappingType.actionType;
            ActionType otherActionType = null;
            if (mapping.target is Agent otherAgent && otherAgent.decider.CurrentMapping != null)
            {
                otherActionType = otherAgent.decider.CurrentMapping.mappingType.actionType;
            }
            
            float value = 0f;

            // The entity could already be in inventory or the Agent could be going to get it in a chained mapping
            if (entity.inEntityInventory == agent)
            {
                // Does it match?
                value = agent.inventoryType.CalcOnActionAttributeTypeModifiers(agent, attributeType, actionType, otherActionType,
                                                                               entity, modifierType, isTarget);

                // TODO: If not if slots got switched could it match?  Possible to look for a switch mapping?

            }
            else
            {
                // TODO: Should I know the slot by now?  I think it has to figure it out before getting here.
                //       Add InventoryTargetSlot to Mapping?
                agent.inventoryType.FindInventorySlotsToClear(agent, entity.entityType, out InventorySlot inventorySlot, 1, true);

                if (inventorySlot == null)
                {
                    Debug.LogError(agent.name + ": ActionTypeModifierTF is unable to find a slot for a possible Entity = " + entity.name);
                    return 0f;
                }

                InventoryType.Item item = new InventoryType.Item()
                {
                    inventorySlot = inventorySlot,
                    entity = entity
                };

                value = agent.inventoryType.CalcOnActionAttributeTypeModifiers(agent, attributeType, actionType, otherActionType, item, modifierType, isTarget);
            }
            // This min and max of the curve define the range of values for normalizing the value
            // For example if the value is 10 and the min is 0 and max is 50 - 1/5 = 0.2 would get input to curve
            // Curves are always 0-1 on input axis (x) and 0-1 on output axis (y)
            if (minMaxCurve == null)
            {
                Debug.LogError("ActionTypeModifierTF is missing a Min Max Curve.  Please Fix.");
                return 0f;
            }
            //Debug.Log(agent.name + ": ActionTypeModifierTF - Entity = " + entity.name + " - value = " + value);
            return minMaxCurve.NormAndEvalIgnoreMinMax(value, minMaxCurve.min, minMaxCurve.max);
        }
    }
}
