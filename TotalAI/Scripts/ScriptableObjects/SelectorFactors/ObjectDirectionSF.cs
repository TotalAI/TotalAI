using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ObjectDirectionSF", menuName = "Total AI/Selector Factor/Object Direction", order = 0)]
    public class ObjectDirectionSF : SelectorFactor
    {
        public void OnEnable()
        {
            editorDescription = "Use this in 2D to determine which direction the object is facing.  Uses the objects name to " +
                                "determine direction - Up, Down, Left, Right.  String Enum should match this in exact order.";
        }

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;

            if (mapping == null || mapping.target == null)
                return float.NegativeInfinity;

            // 4 bins = 0-.25, .25-.5, .5-.75, .75-1
            Entity target = mapping.target;
            if (target.name.Contains("Up"))
                return 0.2f;
            else if (target.name.Contains("Down"))
                return 0.4f;
            else if (target.name.Contains("Left"))
                return 0.6f;
            else if (target.name.Contains("Right"))
                return 0.8f;

            return float.NegativeInfinity;
        }
    }
}