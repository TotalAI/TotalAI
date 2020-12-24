using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NearestInteractionDistanceIST", menuName = "Total AI/Interaction Spot Types/Nearest Interaction Distance", order = 0)]
    public class NearestInteractionDistanceIST : InteractionSpotType
    {
        public int numAttemptsToFindInteractionSpot = 10;

        public override bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping)
        {
            float arriveDistance = agent.movementType.GetArriveDistance(agent, entity, mapping);
            bool closeEnough = arriveDistance + 0.5f > agent.memoryType.KnownEntity(agent, entity).distance;

            Vector3 dir = (entity.transform.position - agent.transform.position).normalized;
            float dotProd = Vector3.Dot(dir, agent.transform.forward);
            bool facingEntity = dotProd > 0.8f;

            Debug.Log("CanBeInteractedWith: agent = " + agent.name + " : entity = " + name + " closeEnough = " +
                      closeEnough + " : facing = " + facingEntity + " : dot = " + dotProd);

            return closeEnough && facingEntity;
        }

        public override float GetDistanceTolerance(Agent agent, Entity entity, Mapping mapping, float defaultTolerance = 1f)
        {
            return defaultTolerance;
        }

        public override Vector3 GetRotateTowardsLocation(Agent agent, Entity entity, Mapping mapping)
        {
            if (entity != null)
                return entity.transform.position;
            return Vector3.positiveInfinity;
        }

        public override Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory)
        {
            float maxDistance;
            if (forInventory)
                maxDistance = entity.entityType.maxDistanceAsInput;
            else if (forGoTo)
                maxDistance = agent.movementType.GetArriveDistance(agent, entity, mapping.parent);
            else
                maxDistance = agent.movementType.GetArriveDistance(agent, entity, mapping);

            // Already within interaction range - just return the agent's current position
            float currentDistance = Vector3.Distance(agent.transform.position, entity.transform.position);
            if (currentDistance < maxDistance)
                return agent.transform.position;

            Vector3 directionToAgent = (agent.transform.position - entity.transform.position).normalized;
            Vector3 interactionSpot = entity.transform.position + maxDistance * directionToAgent;
            bool foundSpot = false;
            for (int i = 0; i < numAttemptsToFindInteractionSpot; i++)
            {
                // Is spot on NavMesh?  Is anything else in the spot?  Is it reachable?
                if (agent.movementType.SamplePosition(interactionSpot, out Vector3 hitLocation) &&
                    agent.movementType.IsSpotAvailable(agent, hitLocation) &&
                    agent.movementType.PathDistance(agent, hitLocation) != -1f)
                {
                    // TODO: Sample a few spots around target - take one with lowest path distance (expensive)
                    interactionSpot = hitLocation;
                    foundSpot = true;

                    // For Debugging
                    entity.goingToLocation = interactionSpot;
                    break;
                }

                // Didn't find a spot try a different potential spot
                Vector3 randomChange = new Vector3(UnityEngine.Random.Range(0.5f, 1f), 0, UnityEngine.Random.Range(0.5f, 1f)).normalized;
                interactionSpot = entity.transform.position + maxDistance * (directionToAgent + randomChange).normalized;
            }

            if (!foundSpot)
            {
                Debug.Log(agent.name + ": No interaction spot found for " + entity.name);
                return Vector3.positiveInfinity;
            }
            return interactionSpot;
        }
    }
}
