using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "QuitEventOCT", menuName = "Total AI/Output Change Types/Quit Event", order = 0)]
    public class QuitEventOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Quit Event</b>: Quits the Agent Event that the Agent is currently in.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None }
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            if (agent.inAgentEvent == null)
                return false;
            agent.QuitEvent();
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0f;
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
