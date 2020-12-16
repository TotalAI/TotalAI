using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AttributeSF", menuName = "Total AI/Selector Factor/Attribute", order = 0)]
    public class AttributeSF : SelectorFactor
    {
        public AttributeType attributeType;

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;
            return minMaxCurve.Eval(agent.NormalizedAttributeLevel(attributeType));

            // What would influence the movement speed?
            // Energy - an attribute level
            // Health - an attribute level
            // Target - are there other agents targeting the same target?
            // ActionType - This will mostly be GoTo but could be Explore/Flee/Wander/etc...
            // Action Level - better at an action the faster they can move?
            // RLevel - hate other agent probably move faster?

        }
    }
}