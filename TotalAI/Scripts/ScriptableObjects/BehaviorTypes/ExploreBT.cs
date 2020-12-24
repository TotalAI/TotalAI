using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ExploreBT", menuName = "Total AI/Behavior Types/Explore", order = 0)]
    public class ExploreBT : BehaviorType
    {
        public class Context
        {
            public float currentSpeed;
            public Vector3 targetPosition;
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
                valueTypes = new Type[] { typeof(MinMaxFloatAT), typeof(MinMaxFloatAT) },
                valueDescriptions = new string[] { "Movement Speed", "Energy Level" }
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
            context.targetPosition = Vector3.positiveInfinity;
            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.movementSpeedAT = (MinMaxFloatAT)valueSelector.attributeType;
            context.energyAT = (MinMaxFloatAT)behaviorContext.attributeSelectors[1].attributeType;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Get location to start exploring
            Vector3 targetPosition = GetExploreLocation(agent, 0);

            // Set destination
            agent.behavior.SetTargetPosition(targetPosition);
            context.targetPosition = targetPosition;

            agent.movementType.SetSpeed(agent, context.currentSpeed);
            agent.movementType.SetStopped(agent, false);
            agent.movementType.SetDestination(agent, targetPosition);
        }

        private Vector3 GetExploreLocation(Agent agent, int recCount)
        {
            // TODO: Maybe some kind of grid system?  Or a better area based model?

            if (recCount > 25) return agent.pastLocations[UnityEngine.Random.Range(0, agent.pastLocations.Count)];

            Vector3 targetPosition;
            // Grab a random point that the agent has visited
            targetPosition = agent.pastLocations[UnityEngine.Random.Range(0, agent.pastLocations.Count)];
            
            targetPosition.x += UnityEngine.Random.Range(-10, 10);
            targetPosition.z += UnityEngine.Random.Range(-10, 10);
            
            // Make sure it is far enough away from any visited point            
            foreach (Vector3 pastLocation in agent.pastLocations)
            {
                if (Vector3.Distance(targetPosition, pastLocation) < 5)
                {
                    targetPosition.x += UnityEngine.Random.Range(-10, 10);
                    targetPosition.z += UnityEngine.Random.Range(-10, 10);
                }
            }

            UnityEngine.AI.NavMeshHit hit;
            if (!UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out hit, 2 * 2, UnityEngine.AI.NavMesh.AllAreas))
            {
                ++recCount;
                return GetExploreLocation(agent, recCount);
            }
            return hit.position;
            
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Burn off energy based on how fast Agent was moving
            // TODO: First time should use afterStartWaitTime
            //context.energyAT.ChangeLevelUsingChangeCurve(agent, context.currentSpeed, context.movementSpeedAT.GetMin(agent),
            //                                             context.movementSpeedAT.GetMax(agent), afterUpdateWaitTime, false);

            Selector valueSelector = context.attributeSelectors[0];
            context.currentSpeed = valueSelector.GetFloatValue(agent, agent.decider.CurrentMapping);
            agent.movementType.SetSpeed(agent, context.currentSpeed);

            // Make sure agent can continue going to destination

        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Finished if the agent has reached target or the target is gone
            //Debug.Log(Vector3.Distance(agent.transform.position, targetPosition));
            if (Vector3.Distance(agent.transform.position, context.targetPosition) < 1f)
            {
                //navMeshAgent.SetDestination(null);
                Debug.Log(agent.name + ": Has reached Explore destination");
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
            // Doesn't matter as this is the default "no plan" Behavior
            return 1f;
        }
    }
}