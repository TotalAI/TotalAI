using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WaitOCT", menuName = "Total AI/Output Change Types/Wait", order = 0)]
    public class WaitOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Wait</b>: The agent will wait for X <b>Seconds or Game Minutes</b>.  This will prevent the Behavior from " +
                              "finishing but the Behavior can still be interrupted.  Set Wait Time to -1 to wait forever.",
                possibleTimings = new OutputChange.Timing[] { OutputChange.Timing.OnAnimationEvent, OutputChange.Timing.Repeating,
                   OutputChange.Timing.OnBehaviorInvoke, OutputChange.Timing.BeforeStart, OutputChange.Timing.OnCollisionEnter,
                   OutputChange.Timing.OnTriggerEnter, OutputChange.Timing.AfterGameMinutes },
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.Selector },
                floatLabel = "Wait Time: -1 to Wait Forever",
                usesBoolValue = true,
                boolLabel = "Time in Game Minutes"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            float waitTime;
            if (outputChange.valueType == OutputChange.ValueType.Selector)
                waitTime = outputChange.selector.GetFloatValue(agent, mapping);
            else
                waitTime = outputChange.floatValue;

            if (outputChange.boolValue && waitTime != -1f)
                waitTime *= agent.timeManager.RealTimeSecondsPerGameMinute();

            Debug.Log(agent.name + ": WaitOCT - waiting for " + waitTime);

            agent.behavior.SetIsWaiting(waitTime);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                  Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
