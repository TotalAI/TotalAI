using UnityEngine;

namespace TotalAI
{
    // Certain conditions must be met for outputs to occur
    [CreateAssetMenu(fileName = "PreviousOCStatusCCT", menuName = "Total AI/Change Condition Types/Previous OC Status", order = 0)]
    public class PreviousOCStatusCCT : ChangeConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Earlier OC Status </b>: Looks at a previous Output Change and succeeds or fails based on that OC's status.  " +
                              "Select None or set value to -1 to always select the previous Output Change.",
                possibleValueTypes = new ChangeCondition.ValueType[] { ChangeCondition.ValueType.None, ChangeCondition.ValueType.IntValue,
                                                                       ChangeCondition.ValueType.Selector },
                usesBoolValue = true,
                boolLabel = "Succeeded?"
            };
        }

        public override bool Check(ChangeCondition changeCondition, OutputChange outputChange, Agent agent,
                                   Entity target, Mapping mapping, float actualAmount)
        {
            int index = -1;
            if (changeCondition.valueType == ChangeCondition.ValueType.Selector)
                index = (int)changeCondition.selector.GetFloatValue(agent, mapping);
            else if (changeCondition.valueType == ChangeCondition.ValueType.IntValue)
                index = changeCondition.intValue;

            OutputChange targetOutputChange;
            if (index == -1)
                targetOutputChange = outputChange.Previous(mapping.mappingType);
            else
                targetOutputChange = mapping.mappingType.outputChanges[index];

            // Find this one in history to see it has run and if so what result was
            // TODO: Reliance on History is bad - Add a structure in Decider?  Or DeciderType?
            // Dictionary<(Mapping, OutputChange), Status> - reset it when new plan starts
            HistoryType.OutputChangeLog outputChangeLog = agent.historyType.GetOutputChangeLog(agent, mapping, targetOutputChange);

            if (outputChangeLog == null)
            {
                Debug.LogError(agent.name + ": PreviousOCStatusCCT - unable to find OC status in HistoryType OutputChangeLog.");
                return false;
            }

            if (outputChangeLog.succeeded == changeCondition.boolValue)
                return true;
            return false;
        }
    }
}