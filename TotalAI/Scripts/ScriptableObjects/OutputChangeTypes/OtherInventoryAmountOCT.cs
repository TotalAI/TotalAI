using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherInventoryAmountOCT", menuName = "Total AI/Output Change Types/Other Inventory Amount", order = 0)]
    public class OtherInventoryAmountOCT : OutputChangeType
    {

        // TODO: Is this used?  Seems like it is just creating inventory in the Target Entity?

        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Change Other Inventory</b>: How much to change selected Entity Type Amount in the Input Entity?",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.OppPrevOutputAmount,
                                                                    OutputChange.ValueType.PrevOutputAmount },
                floatLabel = "Amount To Change",
                usesInventoryTypeGroupMatchIndex = true
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            EntityType inventoryTargetEntityType = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType;
            actualAmount = target.inventoryType.Add(target, inventoryTargetEntityType, 0, null, (int)actualAmount);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.OppPrevOutputAmount)
                return -mapping.previousOutputChangeAmount;
            else if (outputChange.valueType == OutputChange.ValueType.PrevOutputAmount)
                return mapping.previousOutputChangeAmount;

            // TODO: Fix this - Need to figure out how the amount will actually change in the inventory
            //return ((WorldObject)target).ChangeItemAmount((ItemType)outputChange.inputOutputType, outputChange.floatValue, false);
            return outputChange.floatValue;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            // Creating inventory in the Entity Target - go with zero
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
