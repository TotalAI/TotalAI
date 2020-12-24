using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NoneHT", menuName = "Total AI/History Types/None", order = 0)]
    public class NoneHT : HistoryType
    {
        public override float ActualPlanRunningTime(Agent agent, PlansLog plansLog)
        {
            return 0f;
        }

        public override PlansLog FindPlansLogFromMapping(Agent agent, Mapping mapping)
        {
            return null;
        }

        public override PlansLog FindPlansLogFromPlans(Agent agent, Plans plans)
        {
            return null;
        }
        
        public override OutputChangeLog GetLastOutputChangeLog(Agent agent)
        {
            return null;
        }

        public override float GetLastTimePerformedActionType(Agent agent, ActionType actionType)
        {
            return 0f;
        }

        public override OutputChangeLog GetOutputChangeLog(Agent agent, Mapping mapping, OutputChange outputChange)
        {
            return null;
        }

        public override void RecordBehaviorLog(Agent agent, BehaviorRunType runType, BehaviorType behaviorType, bool interruptFromBehavior = false)
        {
        }

        public override void RecordDeciderLog(Agent agent, DeciderRunType runType, Mapping currentMapping, bool interruptFromDecider = false)
        {
        }

        public override void RecordOutputChangeLog(Agent agent, Mapping mapping, OutputChange outputChange, float amount, bool succeeded)
        {
        }

        public override void RecordPlansLog(Agent agent, Dictionary<DriveType, float> driveTypesLevels, Dictionary<DriveType, float> driveTypesRanked, Dictionary<DriveType, Plans> allPlans, DriveType chosenDriveType, int chosenPlanIndex, long timeToPlan)
        {
        }
        
        public override void UpdateLastTimePerformedActionType(Agent agent, ActionType actionType)
        {
        }
    }
}
