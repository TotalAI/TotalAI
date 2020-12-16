using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ExactPositionAndRotationIST", menuName = "Total AI/Interaction Spot Types/Exact Position And Rotation", order = 0)]
    public class ExactPositionAndRotationIST : InteractionSpotType
    {
        // Uses Empty GameObjects nameds "InteractionSpot" and "RotateTowardsSpot"
        // Will align the Agent perfectly to be on the InteractionSpot and forward direction facing RotateTowardsSpot

        public string InteractionSpotName = "InteractionSpot";
        public string RotateTowardsSpotName = "RotateTowardsSpot";

        private Dictionary<Entity, Vector3> interactionSpots;
        private Dictionary<Entity, Vector3> rotateTowardsSpots;

        private void OnEnable()
        {
            interactionSpots = new Dictionary<Entity, Vector3>();
            rotateTowardsSpots = new Dictionary<Entity, Vector3>(); 
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

        // If the WorldObject moves this will need to be called to fix the Spots caches
        public override void ResetSpots(Entity entity)
        {
            foreach (Transform child in entity.transform)
            {
                if (child.name == InteractionSpotName)
                {
                    interactionSpots[entity] = child.position;
                }
                else if (child.name == RotateTowardsSpotName)
                {
                    rotateTowardsSpots[entity] = child.position;
                }
            }
        }

        public override bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping)
        {
            Vector3 interactionSpot = GetInteractionSpot(agent, entity, mapping, true, false);
            bool closeEnough = .1f > Vector3.Distance(agent.transform.position, interactionSpot);

            Vector3 dir = (rotateTowardsSpots[entity] - agent.transform.position).normalized;
            float dotProd = Vector3.Dot(dir, agent.transform.forward);
            bool facingEntity = dotProd > 0.9f;

            Debug.Log("CanBeInteractedWith: agent = " + agent.name + " : entity = " + name + " closeEnough = " +
                      closeEnough + " : facing = " + facingEntity + " : dot = " + dotProd);

            return closeEnough && facingEntity;
        }

        public override float GetDistanceTolerance(Agent agent, Entity entity, Mapping mapping, float defaultTolerance)
        {
            return 0.01f;
        }

        public override Vector3 GetRotateTowardsLocation(Agent agent, Entity entity, Mapping mapping)
        {
            return rotateTowardsSpots[entity];
        }

        public override Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory)
        {
            if (agent.movementType.SamplePosition(interactionSpots[entity], out Vector3 hitLocation, .2f) &&
                agent.movementType.IsSpotAvailable(agent, hitLocation))
            {
                // Calculates NavMeshPath and gets the distance - returns -1f if unreachable
                float distance = agent.movementType.PathDistance(agent, hitLocation);
                if (distance > 0)
                {
                    return hitLocation;
                }
            }

            Debug.Log(agent.name + ": ExactPositionAndRotationIST - No interaction spot found for " + entity.name);
            return Vector3.positiveInfinity;
        }
    }
}
