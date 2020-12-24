using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DamagableICT", menuName = "Total AI/Input Condition Types/Damagable", order = 0)]
    public class DamagableICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Damagable</b>: Can this World Object Type be damaged?",
                usesTypeGroup = true,
                usesBoolValue = true,
                boolLabel = "Not Damagable?"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            WorldObject worldObject = target as WorldObject;
            if (worldObject == null)
            {
                Debug.LogError(agent.name + ": DamagableICT is trying to damage a non-WorldObject - " + target.name);
;               return false;
            }
            return worldObject.IsDamagable == !inputCondition.boolValue;
        }
    }
}
