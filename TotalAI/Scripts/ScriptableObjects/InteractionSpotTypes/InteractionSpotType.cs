using UnityEngine;

namespace TotalAI
{
    public abstract class InteractionSpotType : ScriptableObject
    {
        public abstract bool CanBeInteractedWith(Agent agent, Entity entity, Mapping mapping);
        public abstract Vector3 GetRotateTowardsLocation(Agent agent, Entity entity, Mapping mapping);
        public abstract Vector3 GetInteractionSpot(Agent agent, Entity entity, Mapping mapping, bool forGoTo, bool forInventory);
        public abstract float GetDistanceTolerance(Agent agent, Entity entity, Mapping mapping, float defaultTolerance);

        public virtual void Setup(Entity entity)
        {
        }

        public virtual void ResetSpots(Entity entity)
        {
        }
    }
}
