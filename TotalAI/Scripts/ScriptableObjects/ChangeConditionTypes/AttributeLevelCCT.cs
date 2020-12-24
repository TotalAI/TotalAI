using UnityEngine;

namespace TotalAI
{
    // Certain conditions must be met for outputs to occur
    [CreateAssetMenu(fileName = "AttributeLevelCCT", menuName = "Total AI/Change Condition Types/Attribute Level", order = 0)]
    public class AttributeLevelCCT : ChangeConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Target Agent's Attribute Level Is Less Than</b>: Fix this!",
                possibleValueTypes = new ChangeCondition.ValueType[] { ChangeCondition.ValueType.FloatValue },
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(AttributeType),
                levelTypeLabel = "Attribute Type"
            };
        }

        public override bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount)
        {
            Agent targetAgent = mapping.target as Agent;
            if (targetAgent != null)
            {
                if (targetAgent.attributes[(AttributeType)changeCondition.levelType].GetLevel() < changeCondition.floatValue)
                    return true;
            }
            return false;
        }
    }
}