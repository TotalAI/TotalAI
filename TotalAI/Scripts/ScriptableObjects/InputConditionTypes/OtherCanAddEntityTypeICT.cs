using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherCanAddEntityTypeICT", menuName = "Total AI/Input Condition Types/Other Can Add Entity Type", order = 0)]
    public class OtherCanAddEntityTypeICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Other Can Add Entity Type</b>: Does the Target Entity have a spot for the EntityType?",
                usesTypeGroupFromIndex = true,
                usesEntityType = true
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            return target.inventoryType.FindInventorySlot(target, inputCondition.entityType) != null;
        }
    }
}
