using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WaitEndOCT", menuName = "Total AI/Output Change Types/Wait End", order = 0)]
    public class WaitEndOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Wait End</b>: Ends the Agent from Waiting.  Add a Float Value or a Selector to delay the end.",
                possibleTimings = new OutputChange.Timing[] { OutputChange.Timing.OnAnimationEvent, OutputChange.Timing.Repeating,
                   OutputChange.Timing.OnBehaviorInvoke, OutputChange.Timing.BeforeStart, OutputChange.Timing.OnCollisionEnter,
                   OutputChange.Timing.OnTriggerEnter, OutputChange.Timing.AfterGameMinutes, OutputChange.Timing.OnQuitAgentEvent },
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.Selector },
                selectorLabel = "Optional Delay Before Ending",
                floatLabel = "Optional Delay Before Ending",
                usesBoolValue = true,
                boolLabel = "Delay in Game Minutes"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            float delayTime;
            if (outputChange.valueType == OutputChange.ValueType.Selector)
                delayTime = outputChange.selector.GetFloatValue(agent, mapping);
            else
                delayTime = outputChange.floatValue;

            if (outputChange.boolValue)
                delayTime *= agent.timeManager.RealTimeSecondsPerGameMinute();

            agent.StartCoroutine(DelayEndWaiting(agent, delayTime));
            return true;
        }

        private IEnumerator DelayEndWaiting(Agent agent, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            agent.behavior.SetIsWaiting(0f);
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
