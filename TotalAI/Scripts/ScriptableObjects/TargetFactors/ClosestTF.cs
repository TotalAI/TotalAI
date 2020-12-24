using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ClosestTF", menuName = "Total AI/Target Factors/Closest", order = 0)]
    public class ClosestTF : TargetFactor
    {

        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            if (entity.entityType.interactionSpotType == null)
            {
                Debug.LogError(agent.name + ": ClosestTF: EntityType " + entity.entityType.name + " has no Interaction Spot Type.  Please fix.");
                return -1f;
            }

            // TODO: Should this also return pathDistance?
            Vector3 interactionSpot = entity.entityType.interactionSpotType.GetInteractionSpot(agent, entity, mapping, false, forInventory);
            if (interactionSpot.x == float.PositiveInfinity)
                return -1f;

            float distance = agent.movementType.PathDistance(agent, interactionSpot);
            Debug.Log(agent.name + ": ClosestTF path distance to " + entity.name + " = " + distance);
            if (distance != -1f)
                return minMaxCurve.NormAndEvalIgnoreMinMax(distance, minMaxCurve.min, minMaxCurve.max);

            // TODO: Should path be calculated once and saved?

            // No valid path return -1 to veto this target
            return -1f;
        }
    }
}
