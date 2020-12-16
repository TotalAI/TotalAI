using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class Decider
    {
        private Agent agent;
        private DeciderType deciderType;
        
        public Mapping CurrentMapping { get; private set; }
        public Mapping PreviousMapping { get; private set; }
        public DriveType CurrentDriveType { get; private set; }
        public DriveType PreviousDriveType { get; private set; }
        public Dictionary<DriveType, Plans> AllCurrentPlans { get; private set; }
        public int CurrentPlanIndex { get; private set; }
        public float PlanStartTime { get; private set; }

        public Dictionary<DriveType, float> currentDriveTypesRanked;
        private Plans currentPlans;
        private Plans previousPlans;
        private int previousPlanIndex;

        // As the plan is carried out the utility will change after every mapping
        // TODO: Update this after every Mapping finishes so it stays correct
        private float currentPlanUtility;

        private bool isActing;
        private bool previouslyInterrupted;
        private float lastReplanTime;
        private float mappingStartTime;
        private int animationEventCount;

        // Records the last time an OutputChange was run for the repeating timing type
        private Dictionary<OutputChange, float> lastUpdateTimes;

        public Decider(Agent agent, DeciderType deciderType)
        {
            currentPlans = null;
            previousPlans = null;
            CurrentPlanIndex = -1;
            CurrentMapping = null;
            PreviousMapping = null;
            CurrentDriveType = null;
            isActing = false;
            previouslyInterrupted = false;
            lastReplanTime = 0;
            currentPlanUtility = -1000;
            animationEventCount = 0;

            this.agent = agent;
            this.deciderType = deciderType;
            if (deciderType != null)
                deciderType.Setup(agent);
        }

        // TODO: Are agent.isAlive checks needed?
        public void Run()
        {
            // Everytime decider runs it can do nothing, check to see if it should switch plans, or if doing nothing start a plan
            float lastPlannedAt = lastReplanTime;
            if (isActing && !previouslyInterrupted && deciderType.ShouldInterrupt(agent, CurrentMapping, CurrentDriveType, currentPlanUtility,
                                                                                  lastReplanTime, out lastPlannedAt))
            {
                // End Action and then plan next time - this allows action to end before starting new plan
                InterruptMapping(false);

                // This will allow decider to not choose the same Mapping next time around
                previouslyInterrupted = true;
                return;
            }
            else if (!isActing && deciderType.ShouldPlan(CurrentMapping))
            {
                lastPlannedAt = Time.time;
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                bool foundCompletedPlan = deciderType.GetPlans(agent, previousPlans, previousPlanIndex, previouslyInterrupted,
                                                               out DriveType selectedDriveType, out currentDriveTypesRanked,
                                                               out Dictionary<DriveType, Plans> plans);
                AllCurrentPlans = plans;
                watch.Stop();
                long timeToPlan = watch.ElapsedMilliseconds;

                // If no current plan see if there is a default (idle) plan
                if (selectedDriveType == null && currentPlans == null)
                {
                    PreviousDriveType = null;

                    // TODO: Put this in to prevent AllCurrentPlans from getting wiped for the Tree Plan Editor
                    if (agent.noPlansMappingType != null)
                    {
                        AllCurrentPlans = deciderType.DefaultPlans(agent, out selectedDriveType, out Mapping newMapping);
                        if (selectedDriveType != null && newMapping != null)
                            StartPlan(selectedDriveType, 0, 0, newMapping);
                    }
                }
                else if (selectedDriveType != null)
                {
                    int newPlanIndex = -1;
                    float newPlanUtility;
                    Mapping newMapping = null;
                    if (currentPlans != null)
                    {
                        // TODO: Change plans when between Mappings of a plan - this is NOT TESTED - deciderType.ShouldPlan never does this
                        newPlanIndex = deciderType.MaybeChangePlan(agent, CurrentMapping, selectedDriveType, AllCurrentPlans, timeToPlan,
                                                                   out selectedDriveType, out newMapping, out newPlanUtility);
                    }
                    else
                    {
                        newPlanIndex = deciderType.ChooseNewPlan(agent, CurrentMapping, selectedDriveType, AllCurrentPlans, timeToPlan,
                                                                 out selectedDriveType, out newMapping, out newPlanUtility);
                    }

                    if (newPlanIndex != -1)
                    {
                        StartPlan(selectedDriveType, newPlanIndex, newPlanUtility, newMapping);
                    }
                    else
                    {
                        // Unable to find anything to do - do default MappingType
                        PreviousDriveType = null;

                        // TODO: Put this in to prevent AllCurrentPlans from getting wiped for the Tree Plan Editor
                        if (agent.noPlansMappingType != null)
                        {
                            AllCurrentPlans = deciderType.DefaultPlans(agent, out selectedDriveType, out Mapping defaultMapping);
                            if (selectedDriveType != null && defaultMapping != null)
                                StartPlan(selectedDriveType, 0, 0, defaultMapping);
                        }
                    }
                }

                previouslyInterrupted = false;
            }

            lastReplanTime = lastPlannedAt;

            if (CurrentMapping != null && !isActing) {
                // Don't need to recheck conditions if we just started plan
                if (PlanStartTime == Time.time || deciderType.RecheckInputConditions(agent, CurrentMapping))
                {
                    // See if we want to revaluate the current targets before maybe adding a GoTo and starting the Mapping
                    deciderType.ReevaluateTargets(agent, CurrentMapping);

                    // Adds a GoTo Mapping child if needed
                    CurrentMapping = deciderType.MaybeAddGoToMapping(agent, CurrentMapping);
                    
                    // TODO: This currently always returns true - can it ever return false?
                    isActing = deciderType.StartMapping(agent, CurrentMapping);
                    if (isActing)
                    {
                        mappingStartTime = Time.time;

                        // Will return an empty Dictionary if there are no Repeating and no AfterGameMinutesOCs
                        lastUpdateTimes = CurrentMapping.mappingType.InitLastUpdateTimes();
                        animationEventCount = 0;
                    }
                    else
                    {
                        Debug.Log(agent.name + ": deciderType.StartAction - Failed: " + CurrentMapping);
                    }
                }
                else
                {
                    // Recheck IC failed
                    currentPlans.SetSelectedPlanStatus(Plans.Status.Interrupted, CurrentPlanIndex);
                    currentPlans = null;
                    PreviousMapping = CurrentMapping;
                    CurrentMapping = null;
                }
            }
        }

        private void StartPlan(DriveType selectedDriveType, int newPlanIndex, float newPlanUtility, Mapping newMapping)
        {
            // Starting new plan - either because there was no plan or it decided to change plans between actions
            CurrentDriveType = selectedDriveType;
            currentPlans = AllCurrentPlans[selectedDriveType];
            PreviousMapping = CurrentMapping;
            CurrentMapping = newMapping;
            CurrentPlanIndex = newPlanIndex;
            currentPlanUtility = newPlanUtility;
            PlanStartTime = Time.time;
            
            currentPlans.statuses[CurrentPlanIndex] = Plans.Status.Running;

            // This is for the drive modifyBonus for continuing to reduce the same drive
            PreviousDriveType = CurrentDriveType;
        }

        public void MappingFinished(Entity target, bool wasInterrupted)
        {
            previousPlans = currentPlans;
            previousPlanIndex = CurrentPlanIndex;

            if (wasInterrupted)
            {
                Debug.Log(agent.name + ":" + CurrentMapping.ToString() + " was interrupted and not completed");

                currentPlans.statuses[CurrentPlanIndex] = Plans.Status.Interrupted;
                PreviousMapping = CurrentMapping;
                CurrentMapping = null;
                currentPlans = null;

                isActing = false;
                if (PreviousMapping.mappingType.actionType.noWaitOnInterrupt)
                    agent.RunEarly();
            }
            else if (CurrentMapping != null && CurrentPlanIndex != -1 && currentPlans.rootMappings.Count > CurrentPlanIndex)
            {
                Debug.Log(agent.name + ": " + CurrentMapping.ToString() + " on root mapping " + currentPlans.rootMappings[CurrentPlanIndex] + " has completed.");

                deciderType.MappingFinished(agent, CurrentMapping);

                // Move to the next mapping in the mapping tree
                PreviousMapping = CurrentMapping;
                CurrentMapping = CurrentMapping.NextMapping();

                if (CurrentMapping == null)
                {
                    Debug.Log(agent.name + ": Plan (" + currentPlans.rootMappings[CurrentPlanIndex] + ") Finished - Took " + (Time.time - PlanStartTime) + " seconds.");

                    // Plan has completed
                    currentPlans.statuses[CurrentPlanIndex] = Plans.Status.Finished;
                    currentPlans = null;

                    isActing = false;
                    if (PreviousMapping.mappingType.actionType.noWaitOnFinishNoNextMapping)
                        agent.RunEarly();
                }
                else if (PreviousMapping.mappingType.actionType.noWaitOnFinishHasNextMapping)
                {
                    Debug.Log("*********** RunEarly **************");
                    isActing = false;
                    agent.RunEarly();
                }
                else
                {
                    isActing = false;
                }
            }
            else
            {
                isActing = false;
            }
        }

        public void InterruptMapping(bool cameFromBehavior)
        {
            deciderType.InterruptMapping(agent, CurrentMapping, cameFromBehavior);            
        }

        // TODO: Move this to deciderType? - so it can decide how to deal with repeating OCs
        public void RunRepeatingOutputChanges()
        {
            if (lastUpdateTimes.Count > 0)
            {
                deciderType.RunOutputChanges(agent, CurrentMapping, OutputChange.Timing.Repeating, lastUpdateTimes);
            }
        }

        public void RunAfterGameMinutesOutputChanges()
        {
            if (lastUpdateTimes.Count > 0)
            {
                deciderType.RunOutputChanges(agent, CurrentMapping, OutputChange.Timing.AfterGameMinutes, lastUpdateTimes);
            }
        }
        

        public void RunOutputChangesFromAgent(Agent agent, Entity entity, OutputChange.Timing timing)
        {
            if (entity != null)
                Debug.Log(agent.name + ": RunOutputChangesFromAgent (" + timing + ") called with " + entity.name);
            else
                Debug.Log(agent.name + ": RunOutputChangesFromAgent (" + timing + ")  called with no entity");

            if (agent.isAlive && CurrentMapping != null && CurrentMapping.mappingType != null && 
                (CurrentMapping.target == entity ||
                 timing == OutputChange.Timing.OnAnimationEvent || timing == OutputChange.Timing.OnQuitAgentEvent))
            {
                deciderType.RunOutputChanges(agent, CurrentMapping, timing, null, animationEventCount);

                if (timing == OutputChange.Timing.OnAnimationEvent)
                    animationEventCount++;
            }
        }

        // TODO: Turn these into properties?
        public DeciderType DeciderType()
        {
            return deciderType;
        }

        public Plans CurrentPlans()
        {
            return currentPlans;
        }

        public Plans PreviousPlans()
        {
            return previousPlans;
        }

    }
}
