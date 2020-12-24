using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "NearEntityICT", menuName = "Total AI/Input Condition Types/Near Entity", order = 0)]
    public class NearEntityICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Near Entity</b>: Is Agent near an Entity?  Ignores Entities that are in inventory.",
                usesTypeGroup = true,
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (target.inEntityInventory != null)
                return false;
            return true;
        }
    }
}
