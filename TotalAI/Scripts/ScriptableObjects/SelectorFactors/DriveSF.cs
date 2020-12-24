using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveSF", menuName = "Total AI/Selector Factor/Drive", order = 0)]
    public class DriveSF : SelectorFactor
    {
        public DriveType driveType;

        public enum VetoType { None, GreaterThanOrEqual, LessThanOrEqual }

        [Header("Veto Info")]
        public VetoType vetoType;
        public float vetoValue;

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            float level = agent.drives[driveType].GetLevel();

            overrideFactor = false;
            if ((vetoType == VetoType.GreaterThanOrEqual && level >= vetoValue) ||
                (vetoType == VetoType.LessThanOrEqual && level <= vetoValue))
                overrideFactor = true;

            return minMaxCurve.Eval0to100(agent.drives[driveType].GetLevel());

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