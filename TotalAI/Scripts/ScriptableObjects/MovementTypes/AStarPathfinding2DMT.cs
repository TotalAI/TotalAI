using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AStarPathfinding2DMT", menuName = "Total AI/Movement Types/AStar Pathfinding 2D", order = 0)]
    public class AStarPathfinding2DMT : MovementType
    {
        private Dictionary<Agent, AIPath> navAgents;
        private Dictionary<Agent, Seeker> seekers;
        private int entityLayers;

        private void OnEnable()
        {
            navAgents = new Dictionary<Agent, AIPath>();
            seekers = new Dictionary<Agent, Seeker>();
            entityLayers = LayerMask.GetMask("Agent", "WorldObject", "Area", "AgentEvent");
        }
        
        public override void SetupAgent(Agent agent)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && navAgents != null && navAgents.ContainsKey(agent))
            {
                OnEnable();
            }

            navAgents.Add(agent, agent.GetComponent<AIPath>());
            seekers.Add(agent, agent.GetComponent<Seeker>());
            
            NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
            if (nonRootMotionMovement == null)
                Debug.LogError(agent.name + ": Using AStarPathfinding2DMT with No Root Motion which requires a NonRootMotionMovement component.  " +
                                "Please add to agent.");
            else
                nonRootMotionMovement.Initialize(agent);
        }

        public override void NavMeshAgentUpdateRotation(Agent agent, bool allow)
        {
            //navAgents[agent].ro .updateRotation = allow;
        }

        public override void NavMeshAgentUpdatePosition(Agent agent, bool allow)
        {
            //navAgents[agent].updatePosition = allow;
        }

        public override float Radius(Agent agent)
        {
            return navAgents[agent].radius;
        }

        public override void SetDestination(Agent agent, Vector3 location)
        {
            navAgents[agent].destination = location;
        }

        public override Vector3 DesiredVelocityVector(Agent agent)
        {
            return navAgents[agent].desiredVelocity;
        }

        public override float VelocityMagnitude(Agent agent)
        {
            return navAgents[agent].velocity.magnitude;
        }

        public override Vector3 VelocityVector(Agent agent)
        {
            return navAgents[agent].velocity;
        }

        public override float RemainingDistance(Agent agent)
        {
            return navAgents[agent].remainingDistance;
        }

        public override float StoppingDistance(Agent agent)
        {
            return navAgents[agent].endReachedDistance;
        }

        public override void SetSpeed(Agent agent, float speed)
        {
            navAgents[agent].maxSpeed = speed;
        }

        public override void SetStopped(Agent agent, bool stop)
        {
            navAgents[agent].isStopped = stop;
        }

        public override Vector3 NextPosition(Agent agent)
        {
            return Vector3.zero;
            //return navAgents[agent].
        }

        public override void SetNextPosition(Agent agent, Vector3 position)
        {

        }

        public override void Warp(Agent agent, Vector3 location)
        {

        }

        public override bool IsOnOffMeshLink(Agent agent)
        {
            return false;
        }

        public override void CompleteOffMeshLink(Agent agent)
        {

        }

        public override void AlignForOffMeshLinkTraversal(Agent agent)
        {

        }

        public override IEnumerator TraverseOffMeshLink(Agent agent)
        {
            return null;
        }

        public override float PathDistance(Agent agent, Vector3 location)
        {
            Path path = seekers[agent].StartPath(agent.transform.position, location);
            path.BlockUntilCalculated();
            if (!path.error && path.CompleteState != PathCompleteState.Partial)
                return path.GetTotalLength();
            return -1f;
        }

        public override void UpdateNavMesh()
        {
            if (AstarPath.active != null)
                AstarPath.active.Scan();
        }

        public override void UpdatePartialNavMesh(Bounds bounds)
        {
            AstarPath.active.UpdateGraphs(bounds);
        }

        // Figure out how close to get to the target when doing a GoTo action
        public override float GetArriveDistance(Agent agent, Entity target, Mapping mapping)
        {
            // Chain of priority for distance: MappingType > ActionType > InputOutputType - default to 1.5
            MappingType mappingType = mapping.mappingType;
            float distance = 2.5f;
            if (mappingType.maxDistanceAsInput > 0)
                distance = mappingType.maxDistanceAsInput;
            else if (mappingType.actionType.maxDistanceAsInput > 0)
                distance = mappingType.actionType.maxDistanceAsInput;
            else if (mappingType.outputChanges != null && mappingType.outputChanges.Count > 0 &&
                     mappingType.outputChanges[0].outputChangeType.name == "NearEntityOCT" &&
                     mapping.parent != null && mapping.parent.mappingType.actionType.maxDistanceAsInput > 0)
                distance = mapping.parent.mappingType.actionType.maxDistanceAsInput;
            else if (target != null && target.entityType.maxDistanceAsInput > 0)
                distance = target.entityType.maxDistanceAsInput;
            else if (target != null && target.entityType.maxDistanceAsInput > 0)
                distance = target.entityType.maxDistanceAsInput;

            //Debug.Log("GetArriveDistance for " + passedInTarget.name + " Distance = " + distance);
            return distance;
        }

        public override bool SamplePosition(Vector3 center, out Vector3 hitLocation, float radius = 0.5f)
        {
            NNInfo info = AstarPath.active.GetNearest(center, NNConstraint.Default);

            if (info.node.Walkable)
            {
                hitLocation = info.position;
                return true;
            }

            hitLocation = Vector3.positiveInfinity;
            return false;
        }

        // Returns the direction the agent shoule be looking - returns zero if no direction
        public override Vector3 LookAt(Agent agent)
        {
            if (agent.decider.CurrentMapping != null && agent.decider.CurrentMapping.target != null)
                return ((Vector2)agent.decider.CurrentMapping.target.transform.position - (Vector2)agent.transform.position).normalized;
            return Vector2.zero;
        }

        public override bool IsSpotAvailable(Agent agent, Vector3 location, float sphereRadius = 0.1f, float increaseY = 0.5f)
        {
            Physics2D.queriesHitTriggers = false;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(location, sphereRadius, entityLayers);
            Physics2D.queriesHitTriggers = true;
            foreach (var hitCollider in hitColliders)
            {
                Entity hitEntity = hitCollider.GetComponent<Entity>();
                if (hitEntity != null && hitEntity != agent)
                {
                    Debug.Log("OverlapSphere hit " + hitEntity.name + " this = " + name + " - agent = " + agent.name);
                    return false;
                }
            }
            return true;
        }

        public override void Enable(Agent agent)
        {
            NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
            nonRootMotionMovement.enabled = true;
            navAgents[agent].enabled = true;
        }

        public override void Disable(Agent agent)
        {
            
            NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
            nonRootMotionMovement.enabled = false;
            navAgents[agent].enabled = false;
        }

    }
}
