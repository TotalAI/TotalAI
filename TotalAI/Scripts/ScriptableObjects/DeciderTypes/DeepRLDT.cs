using System.Collections.Generic;
using TotalAI.DeepRL;
using UnityEngine;

namespace TotalAI.DeepRL
{
    [CreateAssetMenu(fileName = "DeepRLDT", menuName = "Total AI/Decider Types/Deep RL", order = 0)]
    public class DeepRLDT : DeciderType
    {
        public DriveType defaultDriveType;

        private Dictionary<Agent, TotalAIMLAgent> mlAgents;

        private void OnEnable()
        {
            mlAgents = new Dictionary<Agent, TotalAIMLAgent>();
        }

        public override void Setup(Agent agent)
        {
            base.Setup(agent);

            TotalAIMLAgent mlAgent = agent.GetComponent<TotalAIMLAgent>();

            if (mlAgent == null)
            {
                Debug.LogError(agent.name + ": DeepRLDT is missing a TotalAIMLAgent Component.  Please add this component to Agent.");
            }
            else
            {
                mlAgents[agent] = mlAgent;
            }
        }
        
        public override bool GetPlans(Agent agent, Plans previousPlans, int previousPlansIndex, bool previouslyInterrupted, out DriveType currentDriveType,
                                      out Dictionary<DriveType, float> currentDriveTypesRanked, out Dictionary<DriveType, Plans> allPlans)
        {
            currentDriveType = defaultDriveType;
            currentDriveTypesRanked = new Dictionary<DriveType, float>() { { defaultDriveType, 1f } };
            allPlans = new Dictionary<DriveType, Plans>();
            Plans currentPlans = null;

            currentPlans = plannerTypes[0].CreatePlansForDriveType(agent, defaultDriveType, false);
            if (currentPlans == null)
                return false;

            allPlans.Add(defaultDriveType, currentPlans);

            //Debug.Log(agent.name + ": GetPlans for " + driveType.name + " found " + currentPlans.rootMappings.Count + " plans.");

            List<Mapping> excludeRootMappings = null;
            if (previouslyInterrupted && previousPlans != null)
                excludeRootMappings = new List<Mapping>() { previousPlans.rootMappings[previousPlansIndex] };

            // TODO: Figure out the statuses mess - when should they be set?
            if (currentPlans.GetCompletePlans(excludeRootMappings).Count > 0)
                return true;

            return false;
        }

        // Request an Action from MLAgents
        protected override Mapping GetBestPlan(Agent agent, Plans plans, out float bestUtility, bool force = true)
        {
            // Need to convert from plans(MappingTypes) to possible Actions
            // Need to mask all MappingTypes that aren't in the plans


            bestUtility = 0f;
            mlAgents[agent].RequestDecision();

            // How to figure out what Action was taken?


            // Convert back from Action to Mapping Type

            return null;
        }

        // Never interrupt for now - figure out interrupts later
        public override bool ShouldInterrupt(Agent agent, Mapping currentMapping, DriveType currentDriveType, float currentPlanUtility,
                                             float lastReplanTime, out float newReplanTime)
        {
            newReplanTime = 0f;
            return false;
        }
    }
}
