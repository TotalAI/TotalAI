using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NextToActivateIST", menuName = "Total AI/Interaction Spot Types/Next To Activate", order = 0)]
    public class NextToActivateIST : InteractionSpotType
    {

        public override void Setup(Entity entity)
        {
            WorldObject worldObject = entity as WorldObject;
            if (worldObject == null)
            {
                Debug.LogError(entity.name + ": NextToCompleteIST can only be used for WorldObjectTypes.");
            }
        }

        public override bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping)
        {
            float arriveDistance = agent.movementType.GetArriveDistance(agent, entity, mapping);
            Vector3 interactionSpot = GetInteractionSpot(agent, entity, mapping, true, false);
            bool closeEnough = arriveDistance > Vector3.Distance(agent.transform.position, interactionSpot);

            Vector3 dir = (interactionSpot - agent.transform.position).normalized;
            float dotProd = Vector3.Dot(dir, agent.transform.forward);
            bool facingEntity = dotProd > 0.8f;

            Debug.Log("CanBeInteractedWith: agent = " + agent.name + " : entity = " + name + " closeEnough = " +
                      closeEnough + " : facing = " + facingEntity + " : dot = " + dotProd);

            return closeEnough && facingEntity;
        }

        public override Vector3 GetRotateTowardsLocation(Agent agent, Entity entity, Mapping mapping)
        {
            return mapping.target.transform.position;
        }

        public override float GetDistanceTolerance(Agent agent, Entity entity, Mapping mapping, float defaultTolerance)
        {
            return 1f;
        }

        public override Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory)
        {
            WorldObject worldObject = entity as WorldObject;
            GameObject gameObject = worldObject.worldObjectType.skinPrefabMappings[worldObject.currentSkinPrefabIndex].NextToBeActivated(worldObject);

            return gameObject.transform.position + Vector3.forward;
        }
    }
}
