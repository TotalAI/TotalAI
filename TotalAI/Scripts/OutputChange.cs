using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [System.Serializable]
    public class OutputChange
    {
        public enum Timing { OnFinish, OnAnimationEvent, Repeating, OnBehaviorInvoke, BeforeStart, OnTriggerEnter,
                             OnCollisionEnter, AfterGameMinutes, OnInterrupt, OnQuitAgentEvent }
        [Tooltip("When during Behavior should this OutputChange occur?  OnAnimationEvent is triggered by an animation event " +
                 "with method name \"AnimationOutputChange\".  OnBehaviorInvoked relies on the behavior code to invoke the OutputChange.")]
        public Timing timing;
        [Tooltip("If Repeating or AfterGameMinutes is selected for timing, how often to repeat or wait in game minutes.")]
        public float gameMinutes;
        [Tooltip("If multiple AnimationEvents, which one to trigger on?  First one is 0.  -1 to trigger on all.")]
        public int onAnimationEventOccurence = 0;
        [Tooltip("For repeating OutputChanges, provide an estimate for the total change amount.")]
        public float changeEstimateForPlanner;

        public enum TargetType { ToSelf, ToEntityTarget, ToInventoryTarget, NewEntity }
        [Tooltip("Does the change occur to the Agent performing the MappingType, to the Entity target, inventory target, or is a new Entity created.")]
        public TargetType targetType;

        public enum ValueType { None, OppPrevOutputAmount, PrevOutputAmount, BoolValue, FloatValue, IntValue, StringValue,
                                EnumValue, ActionSkillCurve, UnityObject, Selector }

        [Tooltip("Which InventoryTypeGroup match from InputConditions to use?  First one is 0.  -1 to use all.")]
        public int inventoryTypeGroupMatchIndex = 0;

        public OutputChangeType outputChangeType;
        public EntityType entityType;
        public LevelType levelType;
        public ValueType valueType;
        public bool boolValue;
        public float floatValue;
        public int intValue;
        public string stringValue;
        public int enumValueIndex;
        public MinMaxCurve actionSkillCurve;
        public Object unityObject;
        public Selector selector;
        
        [Tooltip("All the Conditions need to be true for the change to occur.")]
        public List<ChangeCondition> changeConditions;

        // These cascade - OnTargetGone only fails if target is gone, OnOCCFailed fails if target is gone or OCCFailed, etc...
        public enum StopType { OnChangeFailed, OnOCCFailed, OnTargetGone, None }
        public StopType stopType;
        public bool blockMakeChangeForcedStop;

        public List<int> recheckInputConditionsIndexes;
        public bool stopOnRecheckFail;

        public float CalculateSEUtility(DriveType mainDriveType, Agent agent, Mapping mapping)
        {
            Entity target = GetEntityTarget(agent, mapping);
            float amount = outputChangeType.CalculateAmount(agent, target, this, mapping);
            mapping.previousOutputChangeAmount = amount;

            float utility = outputChangeType.CalculateSEUtility(agent, target, this, mapping, mainDriveType, amount);

            // See if this change will also change any of the usesEquation Drives
            float equationDrivesUtility = 0f;
            foreach (DriveType driveType in agent.ActiveDrives().Keys)
            {
                // If this drive is the main drive for the plan it should not be considered a side-effect
                // it will be considered in the rate of drive change for the plan
                if (driveType.syncType == DriveType.SyncType.Equation && driveType != mainDriveType && driveType.includeInSECalculations)
                {
                    // Only looks at this specfic OutputChange
                    float equationDrivesAmount = driveType.driveTypeEquation.CalculateEquationDriveLevelChange(agent, driveType, mapping, this);

                    // DriveType to figure this change of drive level into the side effect utility
                    // TODO: Just use value for now since the other side effects use value - add in utility later
                    equationDrivesUtility += equationDrivesAmount * (driveType.sideEffectValue < 0.01f ? 1f : driveType.sideEffectValue);
                }
            }
            //Debug.Log(agent.name + ": " + mapping.mappingType.name + ": val: " + value + " amt: " + amount + " equ utl: " + equationDrivesUtility + " tot: " + (value * amount - equationDrivesUtility));
            //Debug.Log(value + " * " + amount + " - " + equationDrivesUtility);

            // Subtract the equationDrivesUtility since a positive value is bad (drive increases) and a negative value is good (drive decreases)
            return utility - equationDrivesUtility;
        }

        // Actually makes the change - returns false if changes where not made for any reason
        public bool MakeChange(Agent agent, Mapping mapping, out float actualAmount, out bool targetGone,
                               out bool outputChangeConditionsFailed, out bool makeChangeForcedStop)
        {
            targetGone = false;
            outputChangeConditionsFailed = false;
            makeChangeForcedStop = false;
            actualAmount = 0f;

            Entity target = GetEntityTarget(agent, mapping);
            if (target == null)
            {
                Debug.Log(agent.name + ": OC.MakeChange - target is null - MappingType = " + mapping.mappingType.name);
                targetGone = true;
                return false;
            }

            actualAmount = outputChangeType.CalculateAmount(agent, target, this, mapping);

            if (!CheckConditions(target, agent, mapping, actualAmount))
            {
                Debug.Log(agent + ": OutputChange.MakeChange - output conditions failed - " + this);
                outputChangeConditionsFailed = true;
                return false;
            }

            Debug.Log(agent.name + ": OutputChange.MakeChange - output conditions suceeded - " + this);
            Debug.Log(agent.name + ": OC MakeChange - " + targetType + ": " + outputChangeType.name + ": " + actualAmount);

            return outputChangeType.MakeChange(agent, target, this, mapping, actualAmount, out makeChangeForcedStop);            
        }

        private Entity GetEntityTarget(Agent agent, Mapping mapping)
        {
            Entity target;
            if (targetType == TargetType.ToEntityTarget)
                target = mapping.target;
            else if (targetType == TargetType.ToInventoryTarget)
                target = mapping.inventoryTargets[inventoryTypeGroupMatchIndex];
            else
                target = agent;

            if (target == null || target.gameObject == null)
            {
                // Somehow the target is gone
                Debug.Log(agent.name + ": OutputChange.GetTarget - target GameObject is gone - " + this);
                return null;
            }

            return target;
        }

        public bool CheckConditions(Entity target, Agent agent, Mapping mapping, float actualAmount)
        {
            foreach (ChangeCondition changeCondition in changeConditions)
            {
                if (!changeCondition.changeConditionType.Check(changeCondition, this, agent, target, mapping, actualAmount))
                {
                    return false;
                }
            }
            return true;
        }

        public int Index(MappingType mappingType)
        {
            return mappingType.outputChanges.IndexOf(this);
        }

        public OutputChange Previous(MappingType mappingType)
        {
            int index = mappingType.outputChanges.IndexOf(this);
            if (index == 0)
                return null;
            return mappingType.outputChanges[index - 1];
        }

        public OutputChange Next(MappingType mappingType)
        {
            int index = mappingType.outputChanges.IndexOf(this);
            if (index + 1 == mappingType.outputChanges.Count)
                return null;
            return mappingType.outputChanges[index + 1];
        }

        public string GetValueAsString()
        {
            switch (valueType)
            {
                case ValueType.OppPrevOutputAmount:
                    return "Opp Prev";
                case ValueType.PrevOutputAmount:
                    return "Prev";
                case ValueType.BoolValue:
                    return "bool = " + boolValue;
                case ValueType.FloatValue:
                    return "float = " + floatValue;
                case ValueType.StringValue:
                    return "string = " + stringValue;
                case ValueType.EnumValue:
                    return "enum = " + enumValueIndex;
                case ValueType.ActionSkillCurve:
                    return "ASC = " + actionSkillCurve;
                case ValueType.UnityObject:
                    return "Obj = " + unityObject.name;
                case ValueType.Selector:
                    return "AVS = " + selector;
            }
            return null;
        }

        public string TimingInitial()
        {
            switch (timing)
            {
                case Timing.OnFinish:
                    return "F";
                case Timing.OnAnimationEvent:
                    return "A";
                case Timing.Repeating:
                    return "R";
                case Timing.OnBehaviorInvoke:
                    return "B";
                case Timing.AfterGameMinutes:
                    return "A";
                case Timing.OnCollisionEnter:
                    return "C";
                case Timing.OnTriggerEnter:
                    return "T";
            }
            return "S";
        }

        public override string ToString()
        {
            string mappingAsString = (outputChangeType != null ? outputChangeType.name : "No OCT");
            mappingAsString += ": " + targetType.ToString();
            //mappingAsString += ": " + (inputOutputType != null ? inputOutputType.name : "No IOT");

            mappingAsString += ": " + GetValueAsString();

            if (changeConditions != null)
            {
                for (int j = 0; j < changeConditions.Count; j++)
                {
                    if (changeConditions[j] != null)
                    {
                        mappingAsString += " (" + changeConditions[j] + ")";
                    }
                }
            }

            return mappingAsString;
        }
    }
}
