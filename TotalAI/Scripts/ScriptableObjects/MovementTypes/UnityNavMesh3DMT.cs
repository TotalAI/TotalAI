using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "UnityNavMesh3DMT", menuName = "Total AI/Movement Types/Unity Nav Mesh 3D", order = 0)]
    public class UnityNavMesh3DMT : MovementType
    {
        public bool useRootMotion;
        public bool navAgentUpdatePosition = true;
        public bool navAgentUpdateRotation = true;

        public float secondsInPathCache = 0.2f;

        private NavMeshSurface surface;
        private Dictionary<Agent, NavMeshAgent> navMeshAgents;
        public class PathCache
        {
            public NavMeshPath path;
            public float distance;
            public float time;
        }
        private Dictionary<Agent, Dictionary<Vector3, PathCache>> pathCache;
        private int entityLayers;

        private void OnEnable()
        {
            navMeshAgents = new Dictionary<Agent, NavMeshAgent>();
            pathCache = new Dictionary<Agent, Dictionary<Vector3, PathCache>>();
            entityLayers = LayerMask.GetMask("Agent", "WorldObject", "AgentEvent");
        }

        public void RemoveStaleInPathCache(Agent agent)
        {
            KeyValuePair<Vector3, PathCache>[] itemsToRemove = pathCache[agent].Where(x => Time.time - secondsInPathCache > x.Value.time).ToArray();
            foreach (KeyValuePair<Vector3, PathCache> item in itemsToRemove)
                pathCache[agent].Remove(item.Key);
        }

        public override void SetupAgent(Agent agent)
        {
            if (surface == null)
            {
                surface = FindObjectOfType<NavMeshSurface>();
                if (surface == null)
                {
                    Debug.LogError(agent.name + ": Is using UnityNavMeshMT which requires a NavMeshSurface in the Scene.  Please add.");
                    return;
                }
            }

            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && navMeshAgents != null && navMeshAgents.ContainsKey(agent))
            {
                OnEnable();
            }

            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                Debug.LogError(agent.name + ": Is using UnityNavMeshMT which requires a NavMeshAgent Component.  Please add to Agent.");
                return;
            }
            else if (Mathf.Abs(navMeshAgent.baseOffset) > .25f)
            {
                Debug.LogWarning("NavMeshAgent has a large baseOffset - Please make sure Agent's root transform is on the NavMesh. " +
                                 "(It should be on the ground at the Agent's feet.)");
            }
            navMeshAgents.Add(agent, navMeshAgent);
            pathCache.Add(agent, new Dictionary<Vector3, PathCache>());

            agent.movementType.NavMeshAgentUpdateRotation(agent, navAgentUpdateRotation);
            agent.movementType.NavMeshAgentUpdatePosition(agent, navAgentUpdatePosition);

            if (useRootMotion)
            {
                RootMotionMovement rootMotionMovement = agent.GetComponent<RootMotionMovement>();
                if (rootMotionMovement == null)
                    Debug.LogError(agent.name + ": Using UnityNavMeshMT with Root Motion which requires a RootMotionMovement component.  " +
                                   "Please add to agent.");
                else
                    rootMotionMovement.Initialize(agent);
            }
            else
            {
                NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
                if (nonRootMotionMovement == null)
                    Debug.LogError(agent.name + ": Using UnityNavMeshMT with No Root Motion which requires a NonRootMotionMovement component.  " +
                                   "Please add to agent.");
                else
                    nonRootMotionMovement.Initialize(agent);
            }
            
        }

        public override void NavMeshAgentUpdateRotation(Agent agent, bool allow)
        {
            navMeshAgents[agent].updateRotation = allow;
        }

        public override void NavMeshAgentUpdatePosition(Agent agent, bool allow)
        {
            navMeshAgents[agent].updatePosition = allow;
        }

        public override float Radius(Agent agent)
        {
            return navMeshAgents[agent].radius;
        }

        public override void SetDestination(Agent agent, Vector3 location)
        {
            navMeshAgents[agent].SetDestination(location);
        }

        public override Vector3 DesiredVelocityVector(Agent agent)
        {
            return navMeshAgents[agent].desiredVelocity;
        }

        public override float VelocityMagnitude(Agent agent)
        {
            return navMeshAgents[agent].velocity.magnitude;
        }

        public override Vector3 VelocityVector(Agent agent)
        {
            return navMeshAgents[agent].velocity;
        }

        public override Vector3 NextPosition(Agent agent)
        {
            return navMeshAgents[agent].nextPosition;
        }

        public override void SetNextPosition(Agent agent, Vector3 position)
        {
            navMeshAgents[agent].nextPosition = position;
        }

        public override void Warp(Agent agent, Vector3 location)
        {
            navMeshAgents[agent].Warp(location);
        }

        public override bool IsOnOffMeshLink(Agent agent)
        {
            return navMeshAgents[agent].isOnOffMeshLink;
        }

        public override void CompleteOffMeshLink(Agent agent)
        {
            navMeshAgents[agent].CompleteOffMeshLink();
        }

        public override void AlignForOffMeshLinkTraversal(Agent agent)
        {
            OffMeshLinkData data = navMeshAgents[agent].currentOffMeshLinkData;
            Vector3 startPos = data.startPos + Vector3.up * navMeshAgents[agent].baseOffset;
            Vector3 endPos = data.endPos + Vector3.up * navMeshAgents[agent].baseOffset;

            // Need to know the type - for now just handle ladders - then doors
            UnityEngine.Object owner = navMeshAgents[agent].navMeshOwner;
            GameObject ladder = (owner as Component).gameObject;

            // Is agent going up or down the ladder?
            bool goingUp = endPos.y - startPos.y > 0f;

            if (goingUp)
            {
                agent.transform.position = ladder.transform.GetChild(0).position;
                agent.transform.LookAt(ladder.transform.GetChild(0));
            }
            else
            {
                agent.transform.position = ladder.transform.GetChild(1).position;
                agent.transform.LookAt(ladder.transform.GetChild(1));
                agent.transform.Rotate(new Vector3(0f, 180f, 0f));
            }
        }

        public override IEnumerator TraverseOffMeshLink(Agent agent)
        {
            OffMeshLinkData data = navMeshAgents[agent].currentOffMeshLinkData;
            Vector3 startPos = data.startPos + Vector3.up * navMeshAgents[agent].baseOffset;
            Vector3 endPos = data.endPos + Vector3.up * navMeshAgents[agent].baseOffset;

            // Need to know the type - for now just handle ladders - then doors
            
            // Is agent going up or down the ladder?
            bool goingUp = endPos.y - startPos.y > 0f;

            if (goingUp)
            {
                agent.animationType.SetBool(agent, "ClimbUp", true);
                while (data.endPos.y - agent.transform.position.y > 0.2f)
                {
                    if (data.endPos.y - agent.transform.position.y < 2f)
                    {
                        agent.animationType.SetBool(agent, "ClimbUp", false);
                    }
                    yield return null;
                }
            }
            else
            {
                agent.animationType.SetBool(agent, "ClimbDown", true);
                while (agent.transform.position.y - data.endPos.y > 0.2f)
                {
                    if (agent.transform.position.y - data.endPos.y < .55f)
                    {
                        agent.animationType.SetBool(agent, "ClimbDown", false);
                    }
                    yield return null;
                }
            }
            navMeshAgents[agent].CompleteOffMeshLink();
        }

        public override float RemainingDistance(Agent agent)
        {
            return navMeshAgents[agent].remainingDistance;
        }

        public override float StoppingDistance(Agent agent)
        {
            return navMeshAgents[agent].stoppingDistance;
        }

        public override void SetSpeed(Agent agent, float speed)
        {
            navMeshAgents[agent].speed = speed;
        }

        public override void SetStopped(Agent agent, bool stop)
        {
            navMeshAgents[agent].isStopped = stop;
        }

        public override float PathDistance(Agent agent, Vector3 location)
        {
            // TODO: Maybe put this on a per agent timer?
            RemoveStaleInPathCache(agent);

            Vector3 roundedLocation = RoundLocation(location);
            if (pathCache[agent].TryGetValue(roundedLocation, out PathCache cache))
            {
                Debug.Log("Using cached value of " + cache.distance + " to get to " + roundedLocation);
                return cache.distance;
            }

            NavMeshPath path = new NavMeshPath();
            if (!navMeshAgents[agent].CalculatePath(roundedLocation, path))
                return -1f;

            float distance = 0f;
            Debug.Log(agent.name + ": PathDistance to " + roundedLocation + " - Path Status " + path.status);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                for (int i = 1; i < path.corners.Length; ++i)
                {
                    distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }

                // Cache path and distance for future reference
                // TODO: Change location to be an area structure - AABB?
                
                pathCache[agent][roundedLocation] = new PathCache() { path = path, distance = distance, time = Time.time };

                return distance;
            }
            pathCache[agent][roundedLocation] = new PathCache() { path = path, distance = -1f, time = Time.time };
            return -1f;
        }

        private Vector3 RoundLocation(Vector3 location)
        {
            location *= 10f;
            location = new Vector3(Mathf.Round(location.x), Mathf.Round(location.y), Mathf.Round(location.z));
            location /= 10f;
            return location;
        }

        public override void UpdateNavMesh()
        {
            if (surface != null)
                surface.UpdateNavMesh(surface.navMeshData);
        }

        public override void UpdatePartialNavMesh(Bounds bounds)
        {
            // Unity NavMesh does not support this - just do full update
            UpdateNavMesh();
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
            if (NavMesh.SamplePosition(center, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                hitLocation = hit.position;
                return true;
            }

            hitLocation = Vector3.positiveInfinity;
            return false;
        }

        public override Vector3 LookAt(Agent agent)
        {
            return Vector3.zero;
        }
        
        public override bool IsSpotAvailable(Agent agent, Vector3 location, float sphereRadius = 0.1f, float increaseY = 0.5f)
        {
            // Move it up a bit off NavMesh to get closer to center of entities
            location.y += increaseY;

            Collider[] hitColliders = Physics.OverlapSphere(location, sphereRadius, entityLayers, QueryTriggerInteraction.Ignore);
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
            if (useRootMotion)
            {
                RootMotionMovement rootMotionMovement = agent.GetComponent<RootMotionMovement>();
                rootMotionMovement.enabled = true;
            }
            else
            {
                NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
                nonRootMotionMovement.enabled = true;
            }
            navMeshAgents[agent].enabled = true;
        }

        public override void Disable(Agent agent)
        {
            if (useRootMotion)
            {
                RootMotionMovement rootMotionMovement = agent.GetComponent<RootMotionMovement>();
                rootMotionMovement.enabled = false;
            }
            else
            {
                NonRootMotionMovement nonRootMotionMovement = agent.GetComponent<NonRootMotionMovement>();
                nonRootMotionMovement.enabled = false;
            }                
            navMeshAgents[agent].enabled = false;
        }

    }
}
