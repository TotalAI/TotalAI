using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RandomUMT", menuName = "Total AI/Utility Modifier Types/Random", order = 0)]
    public class RandomUMT : UtilityModifierType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Random</b>: Returns a Random value based on the Min Max Curve.",
                usesMinMaxCurve = true
            };
        }

        public override float Evaluate(UtilityModifier utilityModifer, Agent agent, Mapping mapping, out bool veto)
        {
            veto = false;
            return utilityModifer.minMaxCurve.EvalRandom();
        }
    }
}