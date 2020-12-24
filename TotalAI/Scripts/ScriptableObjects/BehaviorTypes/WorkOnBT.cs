using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WorkOnBT", menuName = "Total AI/Behavior Types/Work On", order = 0)]
    public class WorkOnBT : BehaviorType
    {
        // Goes through each "WorkSpot" in order - If "LookAt" child of "WorkSpot" exists will look towards that location
        // Plays the State specified in the passed in Selector when agent arrives at a WorkSpot
        // OCs are responsible for changing completion - this way its possible to have Agent do the animation multiple times
        // For example - AddComplete OC could be set to succeed 50% of the time or AnimationEvent Indexes could be set to various numbers
        // BT considers complete to be in the order of the WorkSpots (for example 3/5 complete - will start on the 4th WorkSpot)

        public float distanceTolerance = 1f;

        public class Context
        {
            public Entity target;
            public Transform targetWaypoint;
            public Vector3 targetPosition;
            public float arriveDistance;
            public float currentSpeed;
            public List<Selector> attributeSelectors;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(MinMaxFloatAT), typeof(EnumStringAT) },
                valueDescriptions = new string[] { "Movement Speed", "Animator State To Play" }
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }

            context.currentSpeed = behaviorContext.attributeSelectors[0].GetFloatValue(agent, agent.decider.CurrentMapping);
            context.target = behaviorContext.target;
            context.attributeSelectors = behaviorContext.attributeSelectors;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Start by going to closest waypoint
            float bestDistance = float.PositiveInfinity;
            Transform closestWaypoint = null;
            foreach (Transform child in context.target.transform)
            {
                // TODO: Probably use closest distance based on NavMesh?  Could also avoid square root
                float distance = Vector3.Distance(agent.transform.position, child.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closestWaypoint = child;
                }
            }

            if (closestWaypoint == null)
            {
                Debug.LogError("Waypoint target has no children.");
                return;
            }

            context.targetWaypoint = closestWaypoint;
            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetStopped(agent, false);
            agent.movementType.SetDestination(agent, closestWaypoint.position);
        }
        
        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Make sure agent can continue going to destination
            if (context.target == null || !context.target.gameObject.activeInHierarchy)
                return;

            context.currentSpeed = context.attributeSelectors[0].GetFloatValue(agent, agent.decider.CurrentMapping);
            agent.movementType.SetSpeed(agent, context.currentSpeed);

            // If agent is close enough to waypoint - go to next one
            if (DistanceIgnoreY(agent.transform.position, context.targetWaypoint.position) < distanceTolerance)
            {
                context.targetWaypoint = GetNextWaypoint(context.target.transform, context.targetWaypoint);
                agent.movementType.SetDestination(agent, context.targetWaypoint.position);
            }
        }

        public float DistanceIgnoreY(Vector3 from, Vector3 to)
        {
            from.y = 0;
            to.y = 0;

            return Vector3.Distance(from, to);
        }

        // Cycles through the child waypoints in sibling order
        public Transform GetNextWaypoint(Transform parent, Transform currentChild)
        {
            int currentIndex = currentChild.GetSiblingIndex();
            if (currentIndex == parent.childCount - 1)
                currentIndex = -1;
            return parent.GetChild(currentIndex + 1);
        }

        public override bool IsFinished(Agent agent)
        {
            // Never finished - must be interrupted
            return false;
        }

        public override void InterruptBehavior(Agent agent)
        {
            // Stop moving
            agent.movementType.SetStopped(agent, true);
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            float distance = Vector3.Distance(mapping.target.transform.position, agent.transform.position);
            Selector attributeSelector = mapping.mappingType.GetAttributeSelectors()[0];
            float speed = attributeSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
            float time = distance / speed;

            //Debug.Log("Estimate TimeToComplete: Going to " + mapping.target.name + " Distance: " + distance + " Speed: " + speed + " Time: " + time);

            // Might want to pad this a bit for accel/decel?
            return time;
        }
    }
}
