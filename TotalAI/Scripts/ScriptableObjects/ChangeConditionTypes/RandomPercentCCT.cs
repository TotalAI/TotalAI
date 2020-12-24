using UnityEngine;

namespace TotalAI
{
    // Certain conditions must be met for outputs to occur
    [CreateAssetMenu(fileName = "RandomPercentCCT", menuName = "Total AI/Change Condition Types/Random Percent", order = 0)]
    public class RandomPercentCCT : ChangeConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Random Percent</b>: Rolls a random value between 0-100.  " +
                              "If roll is greater then or equal to the value the check succeeds.",
                possibleValueTypes = new ChangeCondition.ValueType[] { ChangeCondition.ValueType.FloatValue, ChangeCondition.ValueType.Selector }
            };
        }

        public override bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount)
        {
            float probability;
            if (changeCondition.selector.IsValid())
                probability = changeCondition.selector.GetFloatValue(agent, mapping);
            else
                probability = changeCondition.floatValue;

            if (probability >= Random.Range(0f, 100f))
                return true;
            return false;
        }
    }
}