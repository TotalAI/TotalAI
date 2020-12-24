using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "FleeBT", menuName = "Total AI/Behavior Types/Flee", order = 0)]
    public class FleeBT : BehaviorType
    {
        [Header("What DriveType to use to stop fleeing")]
        public DriveType driveType;

        [Header("Drive level needs to get to or below this to stop fleeing")]
        public float driveLevel;

        [Header("If Agent has a Home - Allow them to flee there")]
        public bool allowFleeToHome;

        [Header("RLevel has to be at or below to be considered a threat")]
        public float minHostileRLevel = 30f;

        [Header("Hostile needs to be withing this distance to impact flee direction")]
        public float maxDistanceImpact = 20f;

        [Header("Movement Parameters")]
        public float maxRotationDegreesDelta = 5f;
        public float distanceTolerance = 1f;
        public float rotationTolerance = 1f;
        
        public class Context
        {
            public Entity home;
            public Vector3 targetPosition;
            public float currentSpeed;
            public float lastVelocity;
            public List<Selector> attributeSelectors;
            public MinMaxFloatAT movementSpeedAT;
            //public MinMaxFloatAT energyAT;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                //attributeTypes = new Type[] { typeof(MinMaxFloatAT), typeof(MinMaxFloatAT) },
                //descriptions = new string[] { "Movement Speed", "Energy Level" }
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
            Selector valueSelector = behaviorContext.attributeSelectors[0];
            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.currentSpeed = valueSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
            context.lastVelocity = agent.movementType.VelocityMagnitude(agent);
            context.movementSpeedAT = (MinMaxFloatAT)valueSelector.attributeType;
            //context.energyAT = (MinMaxFloatAT)behaviorContext.attributeSelectors[1].attributeType;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Flee away from hostiles and/or towards Home             
            context.targetPosition = GetFleeLocation(agent);
            if (context.targetPosition.x == float.PositiveInfinity)
                return;

            // Set destination
            // TODO: Send in path since we need to calculate it when picking target
            // TODO: Should always use targetPosition - doesn't make sense to go into target
            // TODO: targetPosition will be PositiveInfinity if it couldn't find a spot - interrupt mapping

            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetStopped(agent, false);
            agent.movementType.SetDestination(agent, context.targetPosition);
            agent.behavior.SetTargetPosition(context.targetPosition);
        }

        // TODO: Need a danger map - then move towards areas of no danger
        public Vector3 GetFleeLocation(Agent agent)
        {
            Vector3 position = Vector3.zero;

            // Just pick a bunch of spots and see which ones are farthest from all hostiles
            // TODO: Memory maintain flee locations?
            foreach (MemoryType.EntityInfo entityInfo in agent.memoryType.GetShortTermMemory(agent))
            {
                // TODO: Need to have a better way to determine hostile - if they are targeting them?
                if (entityInfo.rLevel > minHostileRLevel)
                    continue;

                // Weight the vectors based on distance to agent - Agent wants to flee more strongly away from close hostiles
                // TODO: Make this a curve - Should be very strong impact from close hostiles
                float distanceWeight = Mathf.Max(maxDistanceImpact - entityInfo.distance, 0);
                Vector3 direction = agent.transform.position - entityInfo.lastPos;
                position = position + direction * distanceWeight;
            }

            if (position == Vector3.zero)
            {
                Debug.Log(agent.name + ": Unable to find a flee direction.");
                return Vector3.positiveInfinity;
            }

            bool foundLocation = false;
            int numAttempts = 1;
            Vector3 positionOnNavMesh = Vector3.positiveInfinity;
            Vector3 tempPosition = position;
            while (!foundLocation && numAttempts < 25)
            {
                int randomAngle = UnityEngine.Random.Range(numAttempts * -5, numAttempts * 5);
                tempPosition = Quaternion.Euler(0, randomAngle, 0) * position;

                tempPosition = agent.transform.position + tempPosition.normalized * UnityEngine.Random.Range(5f, 10f);
                foundLocation = agent.movementType.SamplePosition(tempPosition, out positionOnNavMesh, 1f);
                ++numAttempts;
            }

            if (numAttempts == 20)
            {
                Debug.Log(agent.name + ": Unable to find a flee position on NavMesh.");
                return Vector3.positiveInfinity;
            }

            return positionOnNavMesh;
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];
            Selector valueSelector = context.attributeSelectors[0];
            context.currentSpeed = valueSelector.GetFloatValue(agent, agent.decider.CurrentMapping);

            // Burn off energy based on how fast Agent was moving over the distance traveled
            // TODO: First time should use afterStartWaitTime
            float currentVelocity = agent.movementType.VelocityMagnitude(agent);

            // Averaging starting and ending velocity for this time period - should be a good enough estimate
            float averageVelocity = (currentVelocity + context.lastVelocity) / 2f;
            float distanceTraveled = afterUpdateWaitTime * averageVelocity;
            context.lastVelocity = currentVelocity;

            Debug.Log(agent.name + ": Flee Update - Cur Speed: " + context.currentSpeed + " - Avg Vel" + averageVelocity);

            // Energy burned is related to the distanceTraveled and the averageVelocity
            // If averageVelocity is low 

            //context.energyAT.ChangeLevelUsingChangeCurve(agent, context.currentSpeed, context.movementSpeedAT.GetMin(agent),
            //                                             context.movementSpeedAT.GetMax(agent), distanceTraveled, false);

            context.targetPosition = GetFleeLocation(agent);
            if (context.targetPosition.x == float.PositiveInfinity)
                return;

            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetDestination(agent, context.targetPosition);

            // Set in behavior so target gizmo will show correctly
            agent.behavior.SetTargetPosition(context.targetPosition);

            /*
            float distanceToTargetPosition = Vector3.Distance(agent.transform.position, context.targetPosition);

            // Make sure agent can continue going to destination
            if (context.target == null || !context.target.activeInHierarchy)
                return;

            float currentSpeed = context.attributeSelectors[0].GetFloatValue(agent, agent.decider.CurrentMapping);

            context.currentSpeed = currentSpeed;
            agent.navMeshAgent.speed = context.currentSpeed;

            // If target is an agent need to keep updating targetPosition
            //Agent targetAgent = context.target.GetComponent<Agent>();
            //if (context.target != null && targetAgent != null && distanceToTargetPosition > distanceTolerance)
            if (distanceToTargetPosition > distanceTolerance)
            {
                context.targetPosition = context.target.GetComponent<Entity>().GetBestInteractionSpot(agent, agent.decider.CurrentMapping, null, false);
                agent.navMeshAgent.SetDestination(context.targetPosition);

                // Set in behavior so target gizmo will show correctly
                agent.behavior.SetTargetPosition(context.targetPosition);
            }

            // Rotate towards the target at the end
            // TODO: This is jerky - need to figure out how to do this every frame and gradually
            //       Maybe set something on agent and have it run in agent.Update
            if (context.target != null && distanceToTargetPosition < distanceTolerance)
            {
                agent.transform.rotation = GetRotation(agent.transform, context.target.transform);

            }
            */
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            if (context.targetPosition.x == float.PositiveInfinity)
            {
                Debug.Log(agent.name + ": Unable to find a Flee targetPosition - Quitting Flee");
                return true;
            }

            if (Vector3.Distance(agent.transform.position, context.targetPosition) < distanceTolerance)
            {
                Debug.Log(agent.name + ": Has reached the Flee targetPosition - " + context.targetPosition);
                return true;
            }

            return false;
        }

        private Quaternion GetRotation(Transform agentTransform, Transform targetTransform)
        {
            Vector3 dirToTarget = targetTransform.position - agentTransform.position;
            dirToTarget.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            return Quaternion.RotateTowards(agentTransform.rotation, targetRotation, maxRotationDegreesDelta);
        }

        public override void InterruptBehavior(Agent agent)
        {
            agent.movementType.SetStopped(agent, true);
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            return 1f;
        }
    }
}
