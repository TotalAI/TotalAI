using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AlwaysPassICT", menuName = "Total AI/Input Condition Types/Always Pass", order = 0)]
    public class AlwaysPassICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Always Pass</b>: Use if the MappingType should always run.",
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            return true;
        }
    }
}
