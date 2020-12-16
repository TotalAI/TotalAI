using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public abstract class HistoryType : ScriptableObject
    {
        public enum DeciderRunType { Nothing, InterruptedMapping, PlannedNoInterrupt, NewPlan,
                                     SwitchedPlans, StartedMapping, ICRecheckFailed, FinishedMapping }
        public enum BehaviorRunType { Started, Updated, Interrupted, Finished }

        public class PlansLog
        {
            public Dictionary<DriveType, float> driveTypesLevels;
            public Dictionary<DriveType, float> driveTypesRanked;
            public Dictionary<DriveType, Plans> allPlans;
            public DriveType chosenDriveType;
            public int chosenPlanIndex;
            public float time;
            public float timeToPlan;

            public string DisplayAllPlansStats(Agent agent)
            {
                string result = "";

                int i = 1;
                foreach (KeyValuePair<DriveType, Plans> item in allPlans)
                {
                    result += i + ". " + item.Key.name + ": ";
                    i++;
                    int j = 1;
                    foreach (Mapping rootMapping in item.Value.rootMappings)
                    {
                        float rate = -item.Value.driveAmountEstimates[j - 1] / item.Value.timeEstimates[j - 1];

                        float driveUtility = driveTypesRanked[item.Key];

                        if (rootMapping == allPlans[chosenDriveType].rootMappings[chosenPlanIndex])
                            result += "<b>" + j + ". " + rootMapping.mappingType.name + ": " + Math.Round(item.Value.utility[j - 1], 1) + " (" +
                                      Math.Round(item.Value.driveAmountEstimates[j - 1], 1) + ", " +
                                      Math.Round(item.Value.timeEstimates[j - 1], 1) + ", " +
                                      Math.Round(item.Value.sideEffectsUtility[j - 1], 1) + ")</b>";
                                      //agent.utilityFunction.DisplayEquation(driveUtility, rate, item.Value.sideEffectsUtility[j - 1]) + "</b>  ";
                        else
                            result += j + ". " + rootMapping.mappingType.name + ": " + Math.Round(item.Value.utility[j - 1], 1) + " (" +
                                      Math.Round(item.Value.driveAmountEstimates[j - 1], 1) + ", " +
                                      Math.Round(item.Value.timeEstimates[j - 1], 1) + ", " +
                                      Math.Round(item.Value.sideEffectsUtility[j - 1], 1) + ")";
                                      //agent.utilityFunction.DisplayEquation(driveUtility, rate, item.Value.sideEffectsUtility[j - 1]) + "  ";
                        j++;
                    }
                    result += "     ";
                }

                return result;
            }

            public string DisplayDriveTypesRanked()
            {
                string result = "";

                int i = 1;
                foreach (KeyValuePair<DriveType, float> item in driveTypesRanked)
                {
                    if (item.Key == chosenDriveType)
                        result += "<b>" + i + ". " + item.Key.name + ": " + item.Value + "</b>     ";
                    else
                        result += i + ". " + item.Key.name + ": " + item.Value + "     ";
                    i++;
                }

                return result;
            }
        }

        public class OutputChangeLog
        {
            public Mapping mapping;
            public OutputChange outputChange;
            public float time;
            public float amount;
            public bool succeeded;
        }

        public class DeciderLog
        {
            public DeciderRunType runType;
            public float time;
            public Mapping currentMapping;
            public bool interruptFromDecider;
        }

        public class BehaviorLog
        {
            public BehaviorRunType runType;
            public float time;
            public BehaviorType behaviorType;
            public bool interruptFromBehavior;
        }

        public Dictionary<Agent, List<PlansLog>> agentsPlansLogs;
        public Dictionary<Agent, List<OutputChangeLog>> agentsOutputChangeLogs;
        public Dictionary<Agent, List<DeciderLog>> agentsDeciderLogs;
        public Dictionary<Agent, List<BehaviorLog>> agentsBehaviorLogs;

        public Dictionary<Agent, Dictionary<ActionType, float>> lastTimePerformedActionType;

        public virtual void OnEnable()
        {
            agentsPlansLogs = new Dictionary<Agent, List<PlansLog>>();
            agentsOutputChangeLogs = new Dictionary<Agent, List<OutputChangeLog>>();
            agentsDeciderLogs = new Dictionary<Agent, List<DeciderLog>>();
            agentsBehaviorLogs = new Dictionary<Agent, List<BehaviorLog>>();
            lastTimePerformedActionType = new Dictionary<Agent, Dictionary<ActionType, float>>();
        }

        public virtual void SetupAgent(Agent agent)
        {
            // Needed to clear out Dicts after playing for AgentView Editor Window
            if (!Application.isPlaying && agentsPlansLogs != null && agentsPlansLogs.ContainsKey(agent))
            {
                OnEnable();
            }

            agentsPlansLogs.Add(agent, new List<PlansLog>());
            agentsOutputChangeLogs.Add(agent, new List<OutputChangeLog>());
            agentsDeciderLogs.Add(agent, new List<DeciderLog>());
            agentsBehaviorLogs.Add(agent, new List<BehaviorLog>());
            lastTimePerformedActionType.Add(agent, new Dictionary<ActionType, float>());
        }

        public virtual void RecordPlansLog(Agent agent, Dictionary<DriveType, float> driveTypesLevels, Dictionary<DriveType, float> driveTypesRanked,
                                           Dictionary<DriveType, Plans> allPlans, DriveType chosenDriveType, int chosenPlanIndex, long timeToPlan)
        {
            agentsPlansLogs[agent].Add(new PlansLog()
            {
                time = Time.time,
                driveTypesLevels = driveTypesLevels,
                driveTypesRanked = driveTypesRanked,
                allPlans = allPlans,
                chosenDriveType = chosenDriveType,
                chosenPlanIndex = chosenPlanIndex,
                timeToPlan = timeToPlan
            });
        }

        public virtual void RecordOutputChangeLog(Agent agent, Mapping mapping, OutputChange outputChange, float amount, bool succeeded)
        {
            agentsOutputChangeLogs[agent].Add(new OutputChangeLog()
            {
                time = Time.time,
                mapping = mapping,
                outputChange = outputChange,
                amount = amount,
                succeeded = succeeded
            });
        }

        public virtual void RecordDeciderLog(Agent agent, DeciderRunType runType, Mapping currentMapping, bool interruptFromDecider = false)
        {
            agentsDeciderLogs[agent].Add(new DeciderLog()
            {
                time = Time.time,
                runType = runType,
                currentMapping = currentMapping,
                interruptFromDecider = interruptFromDecider

            });
        }

        public virtual void RecordBehaviorLog(Agent agent, BehaviorRunType runType, BehaviorType behaviorType, bool interruptFromBehavior = false)
        {
            agentsBehaviorLogs[agent].Add(new BehaviorLog()
            {
                time = Time.time,
                runType = runType,
                behaviorType = behaviorType,
                interruptFromBehavior = interruptFromBehavior
            });
        }

        public virtual void UpdateLastTimePerformedActionType(Agent agent, ActionType actionType)
        {
            lastTimePerformedActionType[agent][actionType] = Time.time;
        }

        public virtual float GetLastTimePerformedActionType(Agent agent, ActionType actionType)
        {
            if (lastTimePerformedActionType[agent].TryGetValue(actionType, out float time))
                return time;
            return -1f;
        }

        public virtual OutputChangeLog GetOutputChangeLog(Agent agent, Mapping mapping, OutputChange outputChange)
        {
            if (agentsOutputChangeLogs[agent].Count == 0)
                return null;

            List<OutputChangeLog> mappingOutputChangeLogs = agentsOutputChangeLogs[agent].FindAll(x => x.mapping == mapping && x.outputChange == outputChange);

            if (mappingOutputChangeLogs.Count == 0)
                return null;

            return mappingOutputChangeLogs.Last();
        }

        public virtual OutputChangeLog GetLastOutputChangeLog(Agent agent)
        {
            if (agentsOutputChangeLogs[agent].Count == 0)
                return null;

            return agentsOutputChangeLogs[agent].Last();
        }

        public virtual PlansLog FindPlansLogFromMapping(Agent agent, Mapping mapping)
        {
            int num = agentsPlansLogs[agent].Count;
            if (num == 0)
                return null;

            return agentsPlansLogs[agent].Find(
                x => x.allPlans[x.chosenDriveType].rootMappings[x.chosenPlanIndex] == mapping.GetRootMapping());
        }

        public virtual PlansLog FindPlansLogFromPlans(Agent agent, Plans plans)
        {
            int num = agentsPlansLogs[agent].Count;
            if (num == 0)
                return null;
            
            return agentsPlansLogs[agent].Find(x => x.allPlans.Values.Contains(plans));
        }

        // TODO: Should this go inside PlansLog class?
        public virtual float ActualPlanRunningTime(Agent agent, PlansLog plansLog)
        {
            // Find RootMapping of plansLog in the DeciderLog
            // TODO: Add chosenRootMapping to remove these lookups
            Mapping rootMapping = plansLog.allPlans[plansLog.chosenDriveType].rootMappings[plansLog.chosenPlanIndex];
            
            List<DeciderLog> deciderLogs = agentsDeciderLogs[agent].FindAll(
                x => x.currentMapping == rootMapping && (x.runType == DeciderRunType.FinishedMapping || x.runType == DeciderRunType.InterruptedMapping));

            //if (deciderLogs.Count > 1)
            //    Debug.LogError("Both Finished and Interrupted happened!!");

            if (deciderLogs.Count == 0)
                return -1f;

            return deciderLogs[0].time - plansLog.time;
        }
    }
}
