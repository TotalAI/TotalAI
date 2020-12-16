using UnityEngine;

namespace TotalAI
{
    // Certain conditions must be met for outputs to occur
    [CreateAssetMenu(fileName = "GenderCCT", menuName = "Total AI/Change Condition Types/Gender", order = 0)]
    public class GenderCCT : ChangeConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Target Gender</b>: Need enum!  Need Target Type!",
                possibleValueTypes = new ChangeCondition.ValueType[] { ChangeCondition.ValueType.BoolValue },
                boolLabel = "Male?"
            };
        }

        public override bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount)
        { 
            Agent targetAgent = mapping.target as Agent;
            if (targetAgent != null && targetAgent.gender == Agent.Gender.Male && changeCondition.boolValue)
                return true;
            return false;
        }
    }
}