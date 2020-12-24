using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MostItemsTF", menuName = "Total AI/Target Factors/Most Items", order = 0)]
    public class MostItemsTF : TargetFactor
    {
        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            InputCondition inputCondition = mapping.mappingType.EntityTypeInputConditions()[0];

            EntityType entityType = inputCondition.entityType;

            if (entityType == null)
            {
                Debug.LogError("MostItemsFactor requires the inputCondition to have an entityType.");
                return 0f;
            }

            float numItems = entity.inventoryType.GetEntityTypeAmount(entity, entityType);
            return minMaxCurve.NormAndEvalIgnoreMinMax(numItems, minMaxCurve.min, minMaxCurve.max);
        }
    }
}
