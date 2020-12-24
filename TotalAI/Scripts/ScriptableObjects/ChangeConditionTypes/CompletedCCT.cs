using UnityEngine;

namespace TotalAI
{
    // Certain conditions must be met for outputs to occur
    [CreateAssetMenu(fileName = "CompletedCCT", menuName = "Total AI/Change Condition Types/Completed", order = 0)]
    public class CompletedCCT : ChangeConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Completed</b>: Checks the Mapping's Entity Target to see if it is Completed or not.  Will fail if " +
                              "the target is not a World Object.  If the World Object's CompleteType is None the check will succeed.",
                possibleValueTypes = new ChangeCondition.ValueType[] { ChangeCondition.ValueType.BoolValue },
                boolLabel = "Completed?"
            };
        }

        public override bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount)
        { 
            WorldObject worldObject = mapping.target as WorldObject;
            if (worldObject != null && worldObject.isComplete == changeCondition.boolValue)
                return true;
            return false;
        }
    }
}