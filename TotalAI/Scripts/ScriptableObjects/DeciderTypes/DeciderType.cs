using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class DeciderType : ScriptableObject
    {
        public List<PlannerType> plannerTypes;

        public virtual PlannerType GetActivePlannerType(Agent agent)
        {
            return plannerTypes[0];
        }

        public virtual bool GetPlans(Agent agent, Plans previousPlans, int previousPlansIndex, bool previouslyInterrupted,
                                     out DriveType currentDriveType, out Dictionary<DriveType, float> currentDriveTypesRanked,
                                     out Dictionary<DriveType, Plans> allPlans)
        {
            currentDriveType = null;
            Plans currentPlans = null;

            // Find mapping for highest priority drive
            currentDriveTypesRanked = RankDrives(agent.ActiveDrives());

            allPlans = new Dictionary<DriveType, Plans>();
            foreach (DriveType driveType in currentDriveTypesRanked.Keys)
            {
                currentPlans = plannerTypes[0].CreatePlansForDriveType(agent, driveType, false);
                if (currentPlans == null)
                    return false;

                allPlans.Add(driveType, currentPlans);

                //Debug.Log(agent.name + ": GetPlans for " + driveType.name + " found " + currentPlans.rootMappings.Count + " plans.");

                // If there is at least one complete plan we quit early
                // TODO: Improve this - planner should mark the Plans statuses
                // TODO: Add a minimum utility threshold so it doesn't quit and go with a bad plan
                List<Mapping> excludeRootMappings = null;
                if (previouslyInterrupted && previousPlans != null)
                    excludeRootMappings = new List<Mapping>() { previousPlans.rootMappings[previousPlansIndex] };
                if (currentPlans.GetCompletePlans(excludeRootMappings).Count > 0)
                {
                    //Debug.Log(agent.name + ": GetPlans for " + driveType.name + " found a completed plan.");
                    currentDriveType = driveType;
                    return true;
                }
            }
            return false;
        }

        public virtual void Setup(Agent agent)
        {
            // Make sure there is one and only one plannerType
            if (plannerTypes == null || plannerTypes.Count != 1)
            {
                Debug.LogError(agent.name + ": Is using a DeciderType that requires exactly one PlannerType.  Please check " + name + ".");
            }
            else
            {
                plannerTypes[0].Setup(agent);
            }
        }

        // What should agent do when they have no valid plans?
        public virtual Dictionary<DriveType, Plans> DefaultPlans(Agent agent, out DriveType currentDriveType, out Mapping newMapping)
        {
            if (agent.noneDriveType != null && agent.noPlansMappingType != null)
            {
                currentDriveType = agent.noneDriveType;
                newMapping = new Mapping(agent.noPlansMappingType)
                {
                    isComplete = true
                };
                Plans plans = new Plans(currentDriveType, newMapping)
                {
                    statuses = new List<Plans.Status>() { Plans.Status.Complete }
                };
                return new Dictionary<DriveType, Plans>() { { currentDriveType, plans } };
            }

            currentDriveType = null;
            newMapping = null;
            return null;
        }

        // Agent is acting - should the current Mapping be interrupted?
        // This will not start a new plan - it will just cause the current plan to stop and then plan next time
        public virtual bool ShouldInterrupt(Agent agent, Mapping currentMapping, DriveType currentDriveType, float currentPlanUtility,
                                             float lastReplanTime, out float newReplanTime)
        {
            newReplanTime = lastReplanTime;

            if (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.Never)
                return false;

            if (currentMapping.mappingType.actionType.replanFrequency > Time.time - lastReplanTime)
                return false;

            // Replan and see if there is reason to switch up plans
            newReplanTime = Time.time;

            // Find mapping for highest priority drive
            Dictionary<DriveType, float> currentDriveTypesRanked = RankDrives(agent.ActiveDrives());

            Plans currentPlans;
            foreach (DriveType driveType in currentDriveTypesRanked.Keys)
            {
                if ((currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.Always) ||
                    (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.OnlyDrivesThatCanInterrupt && driveType.canCauseInterruptions) ||
                    (currentMapping.mappingType.actionType.interruptType == ActionType.InterruptType.OnlySpecifiedDrives &&
                     currentMapping.mappingType.actionType.interruptingDriveTypes.Contains(driveType)))
                {
                    Debug.Log(agent.name + ": Checking to see if should interrupt " + currentMapping + " for " + driveType);
                    currentPlans = plannerTypes[0].CreatePlansForDriveType(agent, driveType, true);
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

                if (driveType == currentDriveType)
                {
                    // Don't consider DriveTypes ranked lower than the current DriveType
                    break;
                }
            }

            return false;
        }

        // Only plans if there is no next Mapping - Never plans between Mappings in a plan
        public virtual bool ShouldPlan(Mapping currentMapping)
        {            
            if (currentMapping == null)
                return true;
            return false;
        }

        // Choose best plan from highest utility drive
        public virtual int ChooseNewPlan(Agent agent, Mapping currentMapping, DriveType newDriveType, Dictionary<DriveType, Plans> newPlans, long timeToPlan,
                                         out DriveType selectedDriveType, out Mapping newMapping, out float bestUtility)
        {
            newMapping = null;
            selectedDriveType = newDriveType;
            int rootMappingIndex = -1;

            Mapping bestRootMapping = null;
            bestRootMapping = GetBestPlan(agent, newPlans[newDriveType], out bestUtility);

            Debug.Log(agent.name + ": ChooseNewPlan - bestRootMapping = " + bestRootMapping);

            if (bestRootMapping != null)
            {
                rootMappingIndex = newPlans[newDriveType].GetRootMappingIndex(bestRootMapping);
                // Start with first leave node on left most branch
                newMapping = bestRootMapping.GetLeftmostLeaf();

                // Log the Changed Plans
                agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.NewPlan, newMapping);

                // Log the plans
                agent.historyType.RecordPlansLog(agent, null, agent.decider.currentDriveTypesRanked, newPlans, selectedDriveType, rootMappingIndex, timeToPlan);

                return rootMappingIndex;
            }

            return -1;
        }

        // TODO: Should this not change plans if the new plan is the same?
        // Maybe Changes plans when between Mappings of the current plan
        public virtual int MaybeChangePlan(Agent agent, Mapping currentMapping, DriveType newDriveType,
                                            Dictionary<DriveType, Plans> newPlans, long timeToPlan,
                                            out DriveType selectedDriveType, out Mapping newMapping, out float bestUtility)
        {
            newMapping = null;
            selectedDriveType = newDriveType;
            int rootMappingIndex = -1;

            Mapping bestRootMapping = GetBestPlan(agent, newPlans[newDriveType], out bestUtility);
            if (bestRootMapping != null)
            {
                rootMappingIndex = newPlans[newDriveType].GetRootMappingIndex(bestRootMapping);
                // Start with first leave node on left most branch
                newMapping = bestRootMapping.GetLeftmostLeaf();

                // Log the Changed Plans
                agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.SwitchedPlans, newMapping);

                // Log the plans
                agent.historyType.RecordPlansLog(agent, null, agent.decider.currentDriveTypesRanked, newPlans, selectedDriveType, rootMappingIndex, timeToPlan);

                return rootMappingIndex;
            }

            return -1;
        }

        // Returns the rootMapping that has the highest utility out of all rootMappings that are complete (all leaves have all inputs)
        // Use force to allow plans with negative or zero drive rate changes
        protected virtual Mapping GetBestPlan(Agent agent, Plans plans, out float bestUtility, bool force = true)
        {
            // TODO: Move this to top of DeciderType - be nice to be able to set this in the TAI Settings
            bool verboseLogging = false;

            Mapping best = null;
            bestUtility = -1000;
            foreach (Mapping rootMapping in plans.GetCompletePlans())
            {
                if (verboseLogging)
                    Debug.Log(agent.name + ": *** GetBestPlan checking " + rootMapping.mappingType.name + " ***");

                // Returns driveAmount, timeEst, sideEffectsUtility in the float array (only go through tree once)
                float[] planInfo = new float[3];
                rootMapping.CalcUtilityInfoForTree(agent, plans.driveType, planInfo);

                float driveAmount = planInfo[0];
                float timeEst = planInfo[1];
                float sideEffectsUtility = planInfo[2];
                //float driveAmount = rootMapping.CalcDriveChangeForTree(agent, plans.driveType);
                //float timeEst = rootMapping.CalcTimeToCompleteForTree(agent);
                //float sideEffectsUtility = rootMapping.CalcSideEffectsUtilityForTree(agent, plans.driveType);

                float utility = agent.utilityFunction.Evaluate(agent, rootMapping, plans.driveType, driveAmount, timeEst, sideEffectsUtility);

                // TODO: For logging - Maybe move this into UFT?  Should this even be in Plans?
                int rootMappingIndex = plans.rootMappings.IndexOf(rootMapping);
                plans.driveAmountEstimates[rootMappingIndex] = driveAmount;
                plans.timeEstimates[rootMappingIndex] = timeEst;
                plans.sideEffectsUtility[rootMappingIndex] = sideEffectsUtility;
                plans.utility[rootMappingIndex] = utility;

                if (!force && driveAmount >= 0)
                {
                    Debug.Log("DriveType change is greater than or equal to zero (" + driveAmount + ") - skipping it.");
                }
                else
                {
                    if (utility > bestUtility)
                    {
                        best = rootMapping;
                        bestUtility = utility;
                    }
                }

                if (verboseLogging)
                {
                    Debug.Log("Utility Score = " + utility);
                    Debug.Log("Side Effects Utility = " + plans.sideEffectsUtility[rootMappingIndex]);
                    Debug.Log("Time To Complete = " + plans.timeEstimates[rootMappingIndex]);
                    Debug.Log("DriveType Reduction = " + plans.driveAmountEstimates[rootMappingIndex]);
                    Debug.Log(agent.name + ": *** GetBestPlan done checking " + rootMapping.mappingType.name + " ***");
                }
            }
            return best;
        }

        public virtual bool RecheckInputConditions(Agent agent, Mapping mapping)
        {
            // TODO: If needed add in a check to make sure Entity Target is within interaction range


            if (mapping.mappingType.ForEntityType() && mapping.target == null)
            {
                Debug.Log(agent.name + ": " + mapping.mappingType.name + " Recheck Failed! Entity Target is null.");
                agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.ICRecheckFailed, mapping);
                return false;
            }

            // Recheck the input conditions to make sure agent can still do this mapping
            List<InputCondition> failedInputConditions = new List<InputCondition>();
            foreach (InputCondition inputCondition in mapping.mappingType.inputConditions)
            {
                if (!inputCondition.inputConditionType.Check(inputCondition, agent, mapping, mapping.target, true))
                {
                    failedInputConditions.Add(inputCondition);
                }
            }

            if (failedInputConditions.Count > 0)
            {
                foreach (InputCondition inputCondition in failedInputConditions)
                {
                    Debug.Log(agent.name + ": " + mapping.mappingType.name + " Recheck Failed! Failed input condition = " + inputCondition);
                }
                agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.ICRecheckFailed, mapping);
                return false;
            }

            return true;
        }

        public virtual void ReevaluateTargets(Agent agent, Mapping mapping)
        {
            if (mapping.mappingType.reevaluateTargets)
                plannerTypes[0].ReevaluateTargets(agent, mapping);
        }

        public virtual bool RequiresGoToMapping(Agent agent, Mapping mapping)
        {
            // If CurrentMapping has no EntityTypeICs with children add in a GoTo Mapping before it
            return mapping.mappingType.goToActionType != null && !mapping.HasGoToMapping() &&
                mapping.target != null && !mapping.target.entityType.interactionSpotType.CanBeInteractedWith(agent, mapping.target, mapping);
        }

        public virtual Mapping CreateGoToMapping(Mapping mapping)
        {
            MappingType mappingType = CreateInstance<MappingType>();
            mappingType.inputConditions = new List<InputCondition>();
            mappingType.outputChanges = new List<OutputChange>();
            mappingType.actionType = mapping.mappingType.goToActionType;
            mappingType.selectors = mapping.mappingType.goToSelectors;
            mappingType.overrideDefaultSelectors = mapping.mappingType.overrideDefaultGoToSelectors;
            mappingType.name = mappingType.actionType.name + (mapping.target == null ? "Location" : mapping.target.name);

            Mapping goToMapping = new Mapping(mappingType)
            {
                target = mapping.target,
                parent = mapping,
                isComplete = true
            };

            return goToMapping;
        }

        // Adds a GoTo Mapping to an EntityType Mapping if its needed
        public virtual Mapping MaybeAddGoToMapping(Agent agent, Mapping mapping)
        {
            if (RequiresGoToMapping(agent, mapping))
            {
                AgentEvent agentEvent = mapping.target as AgentEvent;
                if (agentEvent != null)
                    agentEvent.NotifyTravellingTo(agent);

                Mapping goToMapping = CreateGoToMapping(mapping);

                if (mapping.children == null)
                {
                    mapping.children = new List<Mapping>() { goToMapping };
                    mapping.reasonForChildren = new List<InputCondition>() { mapping.mappingType.inputConditions.Last() };
                    goToMapping.childIndex = 0;
                }
                else
                {
                    mapping.children.Add(goToMapping);
                    mapping.reasonForChildren.Add(mapping.mappingType.inputConditions.Last());
                    goToMapping.childIndex = mapping.children.Count - 1;
                }
                return goToMapping;
            }
            return mapping;
        }

        public virtual bool StartMapping(Agent agent, Mapping mapping)
        {
            agent.historyType.UpdateLastTimePerformedActionType(agent, mapping.mappingType.actionType);
            agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.StartedMapping, mapping);

            bool wasInterrupted = RunOutputChanges(agent, mapping, OutputChange.Timing.BeforeStart);

            Debug.Log(agent.name + ":" + " Start ActionType: " + mapping.mappingType.actionType.name + " - MappingType: " + mapping.mappingType.name);
            agent.behavior.StartBehavior(mapping.mappingType.actionType, mapping.target, mapping.mappingType.GetAttributeSelectors(), wasInterrupted);

            return true;
        }

        public virtual void MappingFinished(Agent agent, Mapping mapping)
        {
            RunOutputChanges(agent, mapping, OutputChange.Timing.OnFinish);

            // Potentially improves an agent's action skill for the action that just ran
            agent.actions[mapping.mappingType.actionType].MaybeImproveActionSkill(agent);

            agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.FinishedMapping, mapping);
        }
        
        public virtual void InterruptMapping(Agent agent, Mapping currentMapping, bool cameFromBehavior)
        {
            RunOutputChanges(agent, currentMapping, OutputChange.Timing.OnInterrupt);

            agent.historyType.RecordDeciderLog(agent, HistoryType.DeciderRunType.InterruptedMapping, currentMapping, !cameFromBehavior);

            // Interrupt the Mapping if it did not come from the behavior
            if (!cameFromBehavior)
            {
               agent.behavior.InterruptBehavior(false);
            }
        }
        
        // Returns true if Mapping should be interrupted
        public virtual bool RunOutputChanges(Agent agent, Mapping mapping, OutputChange.Timing timing,
                                             Dictionary<OutputChange, float> lastUpdateTimes = null, int animationEventCount = 0)
        {
            if (mapping == null || mapping.mappingType.outputChanges == null)
                return false;

            bool interruptMapping = false;
            foreach (OutputChange outputChange in mapping.mappingType.outputChanges)
            {
                if (outputChange.timing == timing)
                {
                    if (timing == OutputChange.Timing.Repeating)
                    {
                        if (Time.time - lastUpdateTimes[outputChange] < outputChange.gameMinutes * agent.timeManager.RealTimeSecondsPerGameMinute())
                            continue;
                        else
                            lastUpdateTimes[outputChange] = Time.time;
                    }
                    else if (timing == OutputChange.Timing.AfterGameMinutes)
                    {
                        if (Time.time - lastUpdateTimes[outputChange] < outputChange.gameMinutes * agent.timeManager.RealTimeSecondsPerGameMinute())
                            continue;
                        else
                            lastUpdateTimes[outputChange] = float.PositiveInfinity;
                    }
                    else if (timing == OutputChange.Timing.OnAnimationEvent && outputChange.onAnimationEventOccurence != -1 &&
                             animationEventCount != outputChange.onAnimationEventOccurence)
                    {
                        continue;
                    }

                    
                    if (outputChange.recheckInputConditionsIndexes != null && outputChange.recheckInputConditionsIndexes.Count > 0)
                    {
                        foreach (int inputConditionIndex in outputChange.recheckInputConditionsIndexes)
                        {
                            if (inputConditionIndex < 0 || inputConditionIndex >= mapping.mappingType.inputConditions.Count)
                            {
                                Debug.LogError(agent.name + ": OutputChange " + outputChange.outputChangeType.name + " for MappingType " +
                                               mapping.mappingType.name + " has an invalid index for Recheck Input Conditions Indexes.  Please fix.");
                                return true;
                            }
                            else
                            {
                                InputCondition inputCondition = mapping.mappingType.inputConditions[inputConditionIndex];

                                if (!inputCondition.inputConditionType.Check(inputCondition, agent, mapping, mapping.target, true))
                                {
                                    // Recheck failed - interrupt mapping if stopOnRecheckFail
                                    if (outputChange.stopOnRecheckFail)
                                    {
                                        InterruptMapping(agent, mapping, false);
                                        return true;
                                    }
                                    // Just don't do the OutputChange
                                    return false;
                                }
                            }
                        }
                    }

                    bool succeeded = outputChange.MakeChange(agent, mapping, out float amount, out bool targetGone,
                                                             out bool outputChangeConditionsFailed, out bool makeChangeForcedStop);
                    agent.historyType.RecordOutputChangeLog(agent, mapping, outputChange, amount, succeeded);

                    if (makeChangeForcedStop && !outputChange.blockMakeChangeForcedStop)
                    {
                        interruptMapping = true;
                    }
                    else
                    {
                        switch (outputChange.stopType)
                        {
                            case OutputChange.StopType.OnChangeFailed:
                                if (!succeeded)
                                    interruptMapping = true;
                                break;
                            case OutputChange.StopType.OnOCCFailed:
                                if (outputChangeConditionsFailed)
                                    interruptMapping = true;
                                break;
                            case OutputChange.StopType.OnTargetGone:
                                if (targetGone)
                                    interruptMapping = true;
                                break;
                        }
                    }

                    mapping.previousOutputChangeAmount = amount;
                }

                if (interruptMapping)
                {
                    InterruptMapping(agent, mapping, false);
                    return true;
                }
            }

            return false;
        }

        // TODO: Is the Dict guaranteed to preserve the order?
        public virtual Dictionary<DriveType, float> RankDrives(Dictionary<DriveType, Drive> drives)
        {
            Dictionary<DriveType, float> drivesRanked = new Dictionary<DriveType, float>();

            foreach (var drive in drives)
            {
                // Evaluate each Drives utility curve to get utility drive level
                float driveUtility = drive.Value.GetDriveUtility();
                if (driveUtility > 0)
                    drivesRanked.Add(drive.Key, driveUtility);
            }

            return drivesRanked.OrderByDescending(x => x.Value)
                              .ThenByDescending(x => x.Key.priority)
                              .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
