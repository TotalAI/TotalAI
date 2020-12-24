using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "GoToBT", menuName = "Total AI/Behavior Types/GoTo", order = 0)]
    public class GoToBT : BehaviorType
    {
        [Header("Movement Parameters")]
        public float defaultDistanceTolerance = 1f;
        [Tooltip("Make sure to Turn this off for 2D")]
        public bool rotateTowardsTarget = true; 
        public float maxRotationDegreesDelta = 360f;
        public float rotationTolerance = 15f;

        [Header("GoTo Settings")]
        public bool reduceEnergy;
        public bool changeSpeedDuringBehavior;
        public bool recalculateDestination = true;

        public class Context
        {
            public Entity target;
            public Vector3 targetPosition;
            public float distanceTolerance;
            public Vector3 rotateTowardsLocation;
            public float arriveDistance;
            public float currentSpeed;
            public List<Selector> attributeSelectors;
            public MinMaxFloatAT movementSpeedAT;
            public MinMaxFloatAT energyAT;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(MinMaxFloatAT) },
                valueDescriptions = new string[] { "Movement Speed" },
                requiredAttributeTypes = new Type[] { typeof(MinMaxFloatAT) },
                requiredDescriptions = new string[] { "Energy - Only required if Reduce Energy is selected." },
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }
            
            Selector valueSelector = behaviorContext.attributeSelectors[0];
            context.currentSpeed = valueSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
            context.target = behaviorContext.target;
            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.movementSpeedAT = (MinMaxFloatAT)valueSelector.attributeType;
            if (reduceEnergy)
                context.energyAT = (MinMaxFloatAT)requiredAttributeTypes[0];
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Recalculate path in case things have changed
            InteractionSpotType interactionSpotType = context.target.entityType.interactionSpotType;

            context.targetPosition = interactionSpotType.GetInteractionSpot(agent, context.target, agent.decider.CurrentMapping, true, false);
            context.rotateTowardsLocation = interactionSpotType.GetRotateTowardsLocation(agent, context.target, agent.decider.CurrentMapping);
            context.distanceTolerance = interactionSpotType.GetDistanceTolerance(agent, context.target, agent.decider.CurrentMapping,
                                                                                 defaultDistanceTolerance);

            // Set destination
            // TODO: Send in path since we need to calculate it when picking target
            // TODO: Should always use targetPosition - doesn't make sense to go into target
            // TODO: targetPosition will be PositiveInfinity if it couldn't find a spot - interrupt mapping
            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetStopped(agent, false);
            agent.movementType.SetDestination(agent, Target(context.target, context.targetPosition));

            if (agent.animationType.ImplementsAnimationRigging())
            {
                agent.animationType.SetRigWeight(agent, 0, 1);
                agent.animationType.WarpRigConstraintTargetTo(agent, 0, agent.transform.InverseTransformPoint(context.targetPosition),
                                                              Quaternion.identity);
            }
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

            // Burn off energy based on how fast Agent was moving
            // TODO: First time should use afterStartWaitTime
            if (reduceEnergy)
                context.energyAT.ChangeLevelUsingChangeCurve(agent, context.currentSpeed, context.movementSpeedAT, afterUpdateWaitTime, false);

            // TODO: Can't always ignore y - Could be moving under or above the target position
            //float distanceToTargetPosition = Vector3.Distance(agent.transform.position, context.targetPosition);
            float distanceToTargetPosition = DistanceIgnoreY(agent.transform.position, context.targetPosition);

            // Make sure agent can continue going to destination
            if (context.target == null || !context.target.gameObject.activeInHierarchy)
                return;

            if (changeSpeedDuringBehavior)
            {
                Selector valueSelector = context.attributeSelectors[0];
                context.currentSpeed = valueSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
                agent.movementType.SetSpeed(agent, context.currentSpeed);
                Debug.Log(context.currentSpeed);
            }

            if (agent.animationType.ImplementsAnimationRigging())
            {
                agent.animationType.WarpRigConstraintTargetTo(agent, 0, agent.transform.InverseTransformPoint(context.targetPosition),
                                                              Quaternion.identity);
            }

            // If target is an agent need to keep updating targetPosition
            //Agent targetAgent = context.target.GetComponent<Agent>();
            //if (context.target != null && targetAgent != null && distanceToTargetPosition > distanceTolerance)
            if (recalculateDestination && distanceToTargetPosition > context.distanceTolerance)
            {
                // Recalculate path in case things have changed
                InteractionSpotType interactionSpotType = context.target.entityType.interactionSpotType;
                context.targetPosition = interactionSpotType.GetInteractionSpot(agent, context.target, agent.decider.CurrentMapping, true, false);
                agent.movementType.SetDestination(agent, context.targetPosition);

                // Set in behavior so target gizmo will show correctly
                agent.behavior.SetTargetPosition(context.targetPosition);
            }

            // Rotate towards the target at the end
            // TODO: This is jerky - need to figure out how to do this every frame and gradually
            //       Maybe set something on agent and have it run in agent.Update
            if (rotateTowardsTarget && context.target != null && distanceToTargetPosition < context.distanceTolerance)
            {
                agent.transform.rotation = GetRotation(agent.transform, context.rotateTowardsLocation);
            
            }
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Make sure agent can continue going to destination
            if (context.targetPosition.x == float.PositiveInfinity || context.target == null || !context.target.gameObject.activeInHierarchy)
            {
                agent.behavior.InterruptBehavior(true);
                InterruptBehavior(agent);
                Debug.Log(agent.name + ": Target is gone or can't find an Interaction Spot - Interrupting GoTo");
                return false;
            }

            // Finished if the agent has reached target or the target is gone
            if (WithinDistanceIgnoreY(agent.transform.position, context.targetPosition, context.distanceTolerance) &&
                (!rotateTowardsTarget || float.IsPositiveInfinity(context.rotateTowardsLocation.y) ||
                 Quaternion.Angle(agent.transform.rotation, GetRotation(agent.transform, context.rotateTowardsLocation)) < rotationTolerance))
            {
            //if (WithinDistanceIgnoreY(agent.transform.position, context.targetPosition, context.distanceTolerance))
            //{
                //if (rotateTowardsTarget && !float.IsPositiveInfinity(context.rotateTowardsLocation.y))
                //    agent.transform.LookAt(context.rotateTowardsLocation);

                //if (agent.animationType is AnimatorPlusRiggingAT riggingAT)
                //{
                //    riggingAT.SetRigWeight(agent, 0, 0);
                //}

                agent.movementType.SetStopped(agent, true);
                if (context.target != null)
                    Debug.Log(agent.name + ": Has reached the target - " + context.target.name);
                else
                    Debug.Log(agent.name + ": Has reached the targetPosition - " + context.targetPosition);
                return true;
            }

            return false;
        }

        private bool WithinDistanceIgnoreY(Vector3 a, Vector3 b, float distance)
        {
            return DistanceIgnoreY(a, b) < distance;
        }

        private float DistanceIgnoreY(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }

        private Quaternion GetRotation(Transform agentTransform, Vector3 targetLocation)
        {
            Vector3 dirToTarget = targetLocation - agentTransform.position;
            dirToTarget.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            return Quaternion.RotateTowards(agentTransform.rotation, targetRotation, maxRotationDegreesDelta);
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
            float speed = attributeSelector.GetFloatValue(agent, mapping);
            float time = distance / speed;

            Debug.Log(agent.name + ": GoTo Estimate TimeToComplete: Going to " + mapping.target.name +
                      " Distance: " + distance + " Speed: " + speed + " Time: " + time);

            // Might want to pad this a bit for accel/decel?
            return time;
        }
    }
}
