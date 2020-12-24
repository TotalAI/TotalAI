using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NearEntityOCT", menuName = "Total AI/Output Change Types/Near Entity", order = 0)]
    public class NearEntityOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Near Entity</b>: Agent is now near the target Entity Type.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf },
                possibleTimings = new OutputChange.Timing[] { OutputChange.Timing.OnFinish },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None }
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // Near Entity change just passes target up to its parent - agent will be near the target after action (or recheck will fail for next mapping)
            //if (mapping.parent != null)
            //    mapping.parent.target = mapping.target;
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
            List<InputCondition> inputConditionsFromOC = outputChangeMappingType.EntityTypeInputConditions();
            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(inputConditionsFromOC, agent.memoryType.GetKnownEntityTypes(agent));

            if (inputConditionMappingType.AnyEntityTypeMatchesTypeGroups(entityTypes))
                return true;
            return false;
        }
    }
}