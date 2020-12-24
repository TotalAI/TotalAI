using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "UtilityAIDT", menuName = "Total AI/Decider Types/Utility AI", order = 0)]
    public class UtilityAIDT : DeciderType
    {
        public DriveType defaultDriveType;

        public override void Setup(Agent agent)
        {
            base.Setup(agent);

            if (defaultDriveType == null)
            {
                Debug.LogError(agent.name + ": UtilityAIDT is missing a Default Drive Type.  Please fix in its inspector.");
            }
        }

        public override bool GetPlans(Agent agent, Plans previousPlans, int previousPlansIndex, bool previouslyInterrupted,
                                     out DriveType currentDriveType, out Dictionary<DriveType, float> currentDriveTypesRanked,
                                     out Dictionary<DriveType, Plans> allPlans)
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

        // Agent is acting - should the current Mapping be interrupted?
        // This will not start a new plan - it will just cause the current plan to stop and then plan next time
        public override bool ShouldInterrupt(Agent agent, Mapping currentMapping, DriveType currentDriveType, float currentPlanUtility,
                                             float lastReplanTime, out float newReplanTime)
        {
            newReplanTime = lastReplanTime;

            if (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.Never)
                return false;

            if (currentMapping.mappingType.actionType.replanFrequency > Time.time - lastReplanTime)
                return false;

            // Replan and see if there is reason to switch up plans
            newReplanTime = Time.time;

            Plans currentPlans;
            
            if ((currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.Always) ||
                (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.OnlyDrivesThatCanInterrupt &&
                 defaultDriveType.canCauseInterruptions) ||
                (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.OnlySpecifiedDrives &&
                    currentMapping.mappingType.actionType.interruptingDriveTypes.Contains(defaultDriveType)))
            {
                Debug.Log(agent.name + ": Checking to see if should interrupt " + currentMapping + " for " + defaultDriveType);
                currentPlans = plannerTypes[0].CreatePlansForDriveType(agent, defaultDriveType, true);
                Mapping bestRootMapping = GetBestPlan(agent, currentPlans, out float bestUtility);

                // TODO: Should this log the generated Plans?

                // If it finds one plan that is good enough then interrupt
                List<Mapping> rootMappings = currentPlans.GetCompletePlans();
                if (rootMappings.Count > 0)
                {
                    foreach (Mapping rootMapping in rootMappings)
                    {
                        // Since this is in the middle of the action the last planning utility calculation for current plan
                        // Should be good enough - but it might make sense to recalulate the utility for the current plan
                        if (bestUtility - currentPlanUtility > currentMapping.mappingType.actionType.minGreaterUtilityToInterrupt)
                        {
                            Debug.Log(agent.name + ": Found better plan - interrupting " + currentMapping);
                            Debug.Log(agent.name + ": New Root Mapping " + rootMapping);
                            Debug.Log(agent.name + ": Utility change: " + currentPlanUtility + " -> " + bestUtility);
                            plannerTypes[0].NotifyOfInterrupt(agent, currentPlans, rootMapping);
                            return true;
                        }
                        else
                        {
                            Debug.Log(agent.name + ": NOT interrupting " + currentMapping);
                            Debug.Log(agent.name + ": Checked Root Mapping " + rootMapping);
                            Debug.Log(agent.name + ": Utility change: " + currentPlanUtility + " -> " + bestUtility);
                            agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.PlannedNoInterrupt, currentMapping);
                        }
                    }
                }
            }
            
            return false;
        }

        // Returns the rootMapping that has the highest utility out of all rootMappings that are complete (all leaves have all inputs)
        // Use force to allow plans with negative or zero drive rate changes
        protected override Mapping GetBestPlan(Agent agent, Plans plans, out float bestUtility, bool force = true)
        {
            // TODO: Move this to top of DeciderType - be nice to be able to set this in the TAI Settings
            bool verboseLogging = true;

            Mapping best = null;
            bestUtility = -1000;
            foreach (Mapping rootMapping in plans.rootMappings)
            {
                float utility = agent.utilityFunction.Evaluate(agent, rootMapping, plans.driveType, 0f, 0f, 0f);

                // TODO: For logging - Maybe move this into UFT?  Should this even be in Plans?
                int rootMappingIndex = plans.rootMappings.IndexOf(rootMapping);
                plans.driveAmountEstimates[rootMappingIndex] = 0f;
                plans.timeEstimates[rootMappingIndex] = 0f;
                plans.sideEffectsUtility[rootMappingIndex] = 0f;
                plans.utility[rootMappingIndex] = utility;

                if (!force && utility <= 0)
                {
                    Debug.Log("UtilityAIPT.GetBestPlan for " + rootMapping.mappingType.name + " - Utility is <= 0 (" + utility + ") - skipping it.");
                }
                else
                {
                    if (utility > bestUtility)
                    {
                        best = rootMapping;
                        bestUtility = utility;
                    }
                    if (verboseLogging)
                    {
                        Debug.Log("UtilityAIPT.GetBestPlan for " + rootMapping.mappingType.name + " - Utility Score = " + utility);
                    }
                }
            }
            return best;
        }
    }
}
