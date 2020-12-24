using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WorldObjectStateICT", menuName = "Total AI/Input Condition Types/World Object State", order = 0)]
    public class WorldObjectStateICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>World Object State</b>: Is there a World Object Type near the Agent that is in the specified state?",
                usesTypeGroup = true,
                usesStringValue = true,
                stringLabel = "State"
            };
        }
        
        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            WorldObject worldObject = (WorldObject)target;
            if (worldObject.currentState != null && worldObject.currentState.name == inputCondition.stringValue)
                return true;
            return false;
        }
    }
}
