using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ClosestSpotIST", menuName = "Total AI/Interaction Spot Types/Closest Spot", order = 0)]
    public class ClosestSpotIST : InteractionSpotType
    {
        public string InteractionSpotName = "InteractionSpot";

        private Dictionary<Entity, List<Vector3>> interactionSpots;

        private void OnEnable()
        {
            interactionSpots = new Dictionary<Entity, List<Vector3>>();
        }

        public override void Setup(Entity entity)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && interactionSpots != null && interactionSpots.ContainsKey(entity))
            {
                OnEnable();
            }

            ResetSpots(entity);
        }

        // If the World Object moves this will need to be called to fix the Spots caches
        public override void ResetSpots(Entity entity)
        {
            interactionSpots[entity] = new List<Vector3>();

            foreach (Transform child in entity.transform)
            {
                if (child.name == InteractionSpotName)
                {
                    interactionSpots[entity].Add(child.position);
                }
            }
            if (interactionSpots[entity].Count == 0)
                Debug.LogError("ClosestSpotIST: " + entity.name + " has no interaction spots.  Must have child GameObjects named '" +
                                InteractionSpotName + "'");
        }

        public override bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping)
        {
            Vector3 interactionSpot = GetInteractionSpot(agent, entity, mapping, true, false);
            bool closeEnough = .1f > Vector3.Distance(agent.transform.position, interactionSpot);

            Vector3 dir = (entity.transform.position - agent.transform.position).normalized;
            float dotProd = Vector3.Dot(dir, agent.transform.forward);
            bool facingEntity = dotProd > 0.8f;

            Debug.Log("ClosestSpotIST.CanBeInteractedWith: agent = " + agent.name + " : entity = " + name + " closeEnough = " +
                      closeEnough + " : facing = " + facingEntity + " : dot = " + dotProd);

            return closeEnough && facingEntity;
        }

        public override float GetDistanceTolerance(Agent agent, Entity entity, Mapping mapping, float defaultTolerance)
        {
            return 0.01f;
        }

        public override Vector3 GetRotateTowardsLocation(Agent agent, Entity entity, Mapping mapping)
        {
            return entity.transform.position;
        }

        public override Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory)
        {

            Vector3 interactionSpot = Vector3.positiveInfinity;
            float bestDistance = -1f;
            foreach (Vector3 spot in interactionSpots[entity])
            {
                // If agent is already at this return agent's position
                if (Vector3.Distance(agent.transform.position, spot) < 0.1f)
                    return agent.transform.position;

                // Make sure its on NavMesh and if it is get the exact location
                if (agent.movementType.SamplePosition(spot, out Vector3 hitLocation) && agent.movementType.IsSpotAvailable(agent, hitLocation))
                {
                    // Calculates NavMeshPath and gets the distance - returns -1f if unreachable
                    float distance = agent.movementType.PathDistance(agent, hitLocation);
                    if (distance > 0 && (bestDistance == -1f || distance < bestDistance))
                    {
                        interactionSpot = hitLocation;
                        bestDistance = distance;
                    }
                }
            }

            if (interactionSpot.x == float.PositiveInfinity)
                Debug.Log(agent.name + ": ClosestSpotIST - No interaction spot found for " + entity.name);
            return interactionSpot;
        }
    }
}
