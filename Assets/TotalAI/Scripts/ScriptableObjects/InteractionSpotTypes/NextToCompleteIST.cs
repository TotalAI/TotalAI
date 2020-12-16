using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NextToCompleteIST", menuName = "Total AI/Interaction Spot Types/Next To Complete", order = 0)]
    public class NextToCompleteIST : InteractionSpotType
    {
        // Uses Empty GameObjects nameds "InteractionSpot"
        // Goes in order of GameObjects based on the complete points of World Object
        // Can Loop around, ping-pong, or stay at last spot if there are more complete points than InteractionSpots

        public string emptyGameObjectName = "InteractionSpot";
        public enum CycleType { Loop, PingPong, StayAtLastSpot }
        public CycleType cycleType;

        private Dictionary<WorldObject, List<Vector3>> interactionSpots;

        private void OnEnable()
        {
            interactionSpots = new Dictionary<WorldObject, List<Vector3>>();
        }

        public override void Setup(Entity entity)
        {
            WorldObject worldObject = entity as WorldObject;
            if (worldObject == null)
            {
                Debug.LogError(entity.name + ": NextToCompleteIST can only be used for WorldObjectTypes.");
            }

            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && interactionSpots != null && interactionSpots.ContainsKey(worldObject))
            {
                OnEnable();
            }

            ResetSpots(entity);
        }

        // If the WorldObject moves this will need to be called to fix the interactionSpots cache
        public override void ResetSpots(Entity entity)
        {
            WorldObject worldObject = entity as WorldObject;

            List<Vector3> spots = new List<Vector3>();
            foreach (Transform child in worldObject.transform)
            {
                if (child.name == emptyGameObjectName)
                {
                    spots.Add(child.position);
                }
            }
            interactionSpots[worldObject] = spots;
        }

        public override bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping)
        {
            float arriveDistance = agent.movementType.GetArriveDistance(agent, entity, mapping);
            Vector3 interactionSpot = GetInteractionSpot(agent, entity, mapping, true, false);
            bool closeEnough = arriveDistance > Vector3.Distance(agent.transform.position, interactionSpot);

            Vector3 dir = (entity.transform.position - agent.transform.position).normalized;
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
            return 0.01f;
        }

        public override Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory)
        {
            WorldObject worldObject = entity as WorldObject;
            int complete = (int)worldObject.completePoints;
            int spotIndex = complete;
            int numSpots = interactionSpots[worldObject].Count;
            // complete starts at zero - so if complete == numSpots we need to use cycle logic
            if (complete >= numSpots)
            {
                switch (cycleType)
                {
                    case CycleType.Loop:
                        spotIndex = complete % numSpots;
                        break;
                    case CycleType.PingPong:
                        // Example: 0, 1, 2, 3, 4, 3, 2, 1, 0, 1, 2, ...
                        bool increase = true;
                        spotIndex = numSpots - 1;
                        for (int i = numSpots; i <= complete; i++)
                        {
                            if (spotIndex == 0 || spotIndex == numSpots - 1)
                                increase = !increase;
                            if (increase)
                                ++spotIndex;
                            else
                                --spotIndex;
                        }
                        break;
                    case CycleType.StayAtLastSpot:
                        spotIndex = numSpots - 1;
                        break;
                }
            }

            return interactionSpots[worldObject][spotIndex];
        }
    }
}
