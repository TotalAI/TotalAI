using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "GoThroughBT", menuName = "Total AI/Behavior Types/GoThrough", order = 0)]
    public class GoThroughBT : BehaviorType
    {
        [Header("Movement Parameters")]
        public float distanceTolerance = 1f;

        public class Context
        {
            public Entity target;
            public Vector3 targetPosition;
            public float currentSpeed;
            public List<Selector> attributeSelectors;
            public MinMaxFloatAT movementSpeedAT;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(MinMaxFloatAT) },
                valueDescriptions = new string[] { "Movement Speed" }
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }
            Selector attributeSelector = behaviorContext.attributeSelectors[0];
            context.target = behaviorContext.target;
            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.movementSpeedAT = (MinMaxFloatAT)behaviorContext.attributeSelectors[0].attributeType;
            context.currentSpeed = attributeSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Recalculate path in case things have changed
            context.targetPosition = context.target.entityType.interactionSpotType.GetInteractionSpot(agent, context.target, agent.decider.CurrentMapping,
                                                                                                      true, false);
            // Set destination
            // TODO: Send in path since we need to calculate it when picking target
            // TODO: Should always use targetPosition - doesn't make sense to go into target
            // TODO: targetPosition will be PositiveInfinity if it couldn't find a spot - interrupt mapping
            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetStopped(agent, false);
            agent.movementType.SetDestination(agent, Target(context.target, context.targetPosition));
        }

        private Vector3 Target(Entity target, Vector3 targetPosition)
        {
            if (targetPosition.x != float.PositiveInfinity)
            {
                return targetPosition;
            }
            return target.transform.position;
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            float distanceToTargetPosition = Vector3.Distance(agent.transform.position, context.targetPosition);

            // Make sure agent can continue going to destination
            if (context.target == null || !context.target.gameObject.activeInHierarchy)
                return;

            Selector attributeSelector = context.attributeSelectors[0];
            context.currentSpeed = attributeSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
            agent.movementType.SetSpeed(agent, context.currentSpeed);

            // If target is an agent need to keep updating targetPosition
            //Agent targetAgent = context.target.GetComponent<Agent>();
            //if (context.target != null && targetAgent != null && distanceToTargetPosition > distanceTolerance)
            if (distanceToTargetPosition > distanceTolerance)
            {
                // Recalculate path in case things have changed
                context.targetPosition = context.target.entityType.interactionSpotType.GetInteractionSpot(agent, context.target, agent.decider.CurrentMapping,
                                                                                                          true, false);

                // Want to go through the target - so set destination on the other side of the target
                Vector3 throughTargetPosition = context.targetPosition;// + agent.movementType.VelocityVector(agent).normalized * 2;

                agent.movementType.SetDestination(agent, throughTargetPosition);

                // Set in behavior so target gizmo will show correctly
                agent.behavior.SetTargetPosition(context.targetPosition);
            }
            
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Make sure agent can continue going to destination - Keep going if agent has picked up target (go through)
            if (context.target == null || (!context.target.gameObject.activeInHierarchy && context.target.inEntityInventory != agent))
            {
                agent.behavior.InterruptBehavior(true);
                InterruptBehavior(agent);
                Debug.Log(agent.name + ": Target is gone - Interrupting GoThroughBT");
                return false;
            }

            // Finished if the agent has reached target or the target is in agent's inventory
            if (Vector3.Distance(agent.transform.position, context.targetPosition) < distanceTolerance || context.target.inEntityInventory == agent)
            {
                //navMeshAgent.SetDestination(null);
                if (context.target != null)
                    Debug.Log(agent.name + ": Has reached the target - " + context.target.name);
                else
                    Debug.Log(agent.name + ": Has reached the targetPosition - " + context.targetPosition);
                return true;
            }

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
