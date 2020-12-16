using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RLevelOCT", menuName = "Total AI/Output Change Types/RLevel", order = 0)]
    public class RLevelOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.ActionSkillCurve },
                floatLabel = "RLevel Change Amount",
                actionSkillCurveLabel = "RLevel Change From Action Skill"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            if (mapping.target is Agent)
            {
                // TODO: This is really confusing - fix it and make sure it works
                Agent otherAgent = target as Agent;
                if (target == agent)
                    otherAgent = (Agent)mapping.target;
                ((Agent)target).memoryType.ChangeRLevel(((Agent)target), otherAgent, actualAmount);
            }
            else if (mapping.target is AgentEvent)
            {
                // TODO: Move this into AgentEvent so that it can decide how to handle RLevel changes
                // Change this agent's R Level for all other attendees in this event
                AgentEvent agentEvent = (AgentEvent)mapping.target;

                foreach (AgentEvent.Attendee attendee in agentEvent.attendees)
                {
                    if (agent != attendee.agent)
                        agent.memoryType.ChangeRLevel(agent, attendee.agent, actualAmount);
                }
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            if (outputChange.valueType == OutputChange.ValueType.ActionSkillCurve)
                return outputChange.actionSkillCurve.Eval0to100(agent.ActionSkillWithItemModifiers(mapping.mappingType.actionType));
            return outputChange.floatValue;
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
