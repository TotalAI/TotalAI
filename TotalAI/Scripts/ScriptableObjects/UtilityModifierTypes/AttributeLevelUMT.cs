using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AttributeLevelUMT", menuName = "Total AI/Utility Modifier Types/Attribute Level", order = 0)]
    public class AttributeLevelUMT : UtilityModifierType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Attribute Level</b>: Returns the normalized level of the specified Attribute Type.  " +
                              " If Min Max Curve is set will pass it into curve and return curve result.",
                usesLevelType = true,
                levelTypeLabel = "Attribute Type",
                mostRestrictiveLevelType = typeof(AttributeType)
            };
        }

        public override float Evaluate(UtilityModifier utilityModifer, Agent agent, Mapping mapping, out bool veto)
        {
            veto = false;
            float level = agent.attributes[(AttributeType)utilityModifer.levelType].GetNormalizedLevel();
            if (utilityModifer.minMaxCurve != null)
                level = utilityModifer.minMaxCurve.EvalIgnoreMinMax(level);
            return level;
        }
    }
}