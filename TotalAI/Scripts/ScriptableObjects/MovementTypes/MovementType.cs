using System.Collections;
using UnityEngine;

namespace TotalAI
{
    public abstract class MovementType : ScriptableObject
    {
        public abstract void SetupAgent(Agent agent);

        public abstract void NavMeshAgentUpdateRotation(Agent agent, bool allow);
        public abstract void NavMeshAgentUpdatePosition(Agent agent, bool allow);
        public abstract float Radius(Agent agent);

        public abstract void SetDestination(Agent agent, Vector3 location);

        public abstract Vector3 DesiredVelocityVector(Agent agent);

        public abstract float VelocityMagnitude(Agent agent);
        public abstract Vector3 VelocityVector(Agent agent);

        public abstract Vector3 NextPosition(Agent agent);
        public abstract void SetNextPosition(Agent agent, Vector3 position);
        public abstract void Warp(Agent agent, Vector3 location);

        public abstract bool IsOnOffMeshLink(Agent agent);
        public abstract void CompleteOffMeshLink(Agent agent);
        public abstract void AlignForOffMeshLinkTraversal(Agent agent);
        public abstract IEnumerator TraverseOffMeshLink(Agent agent);

        public abstract float RemainingDistance(Agent agent);
        public abstract float StoppingDistance(Agent agent);

        public abstract void SetSpeed(Agent agent, float speed);
        public abstract void SetStopped(Agent agent, bool stop);

        public abstract float PathDistance(Agent agent, Vector3 location);

        public abstract void UpdateNavMesh();
        public abstract void UpdatePartialNavMesh(Bounds bounds);

        public abstract float GetArriveDistance(Agent agent, Entity target, Mapping mapping);
        public abstract bool SamplePosition(Vector3 center, out Vector3 hitLocation, float radius = 0.5f);
        public abstract bool IsSpotAvailable(Agent agent, Vector3 location, float sphereRadius = 0.2f, float increaseY = 0.5f);

        public abstract Vector3 LookAt(Agent agent);

        public abstract void Enable(Agent agent);
        public abstract void Disable(Agent agent);
    }
}