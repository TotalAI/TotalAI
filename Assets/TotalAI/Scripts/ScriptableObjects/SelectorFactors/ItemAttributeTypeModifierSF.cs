using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ItemAttributeTypeModifierSF", menuName = "Total AI/Selector Factor/Item Attribute Type Modifier", order = 0)]
    public class ItemAttributeTypeModifierSF : SelectorFactor
    {
        public bool isTarget;

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;

            if (selector.valueType != Selector.ValueType.AttributeType)
            {
                Debug.LogError(agent + ": ItemAttributeTypeModifierSF - Called with a Selector that is not for an Attribute Type.  Please fix.");
                return float.NegativeInfinity;
            }

            ActionType actionType = mapping.mappingType.actionType;
            ActionType otherActionType = null;
            Agent targetAgent = agent;
            if (mapping.target is Agent otherAgent)
            {
                if (isTarget)
                {
                    targetAgent = otherAgent;
                    otherActionType = mapping.mappingType.actionType;
                    if (targetAgent.decider.CurrentMapping != null)
                        actionType = targetAgent.decider.CurrentMapping.mappingType.actionType;
                    else
                        return float.NegativeInfinity;
                }
                else if (otherAgent.decider.CurrentMapping != null)
                {
                    otherActionType = otherAgent.decider.CurrentMapping.mappingType.actionType;
                }
            }

            return targetAgent.inventoryType.GetItemAttributeTypeModifiersValue(targetAgent, selector.attributeType, actionType,
                                                                                otherActionType, isTarget);
        }
    }
}