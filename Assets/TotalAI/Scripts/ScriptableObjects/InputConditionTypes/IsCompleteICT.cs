using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "IsCompleteICT", menuName = "Total AI/Input Condition Types/Is Complete", order = 0)]
    public class IsCompleteICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Not Complete</b>: Is World Object Type completed?",
                usesTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Is Complete?"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            WorldObject worldObject = (WorldObject)target;
            if ((!worldObject.isComplete && !inputCondition.boolValue) ||
                (worldObject.isComplete && inputCondition.boolValue))
                return true;
            return false;
        }
    }
}
