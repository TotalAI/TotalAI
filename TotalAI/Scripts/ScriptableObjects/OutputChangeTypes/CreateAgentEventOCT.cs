using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "CreateAgentEventOCT", menuName = "Total AI/Output Change Types/Create Agent Event", order = 0)]
    public class CreateAgentEventOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Create Agent Event</b>: Creates a new AgentEvent and joins it immediately.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.NewEntity },
                possibleTimings = new OutputChange.Timing[] { OutputChange.Timing.BeforeStart },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
                usesEntityType = true,
                mostRestrictiveEntityType = typeof(AgentEventType),
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            AgentEventType agentEventType = outputChange.entityType as AgentEventType;

            Vector3 position = target.transform.position;
            if (agentEventType.prefabVariants == null || agentEventType.prefabVariants.Count == 0)
            {
                Debug.LogError(agent.name + ": CreateAgentEventOCT: " + agentEventType.name + " has no Prefab Variants.  Please Fix.");
                return false;
            }
            GameObject newGameObject = agentEventType.CreateEntity(0, position, agentEventType.prefabVariants[0].transform.rotation,
                agentEventType.prefabVariants[0].transform.localScale, agent);
            
            AgentEvent agentEvent = newGameObject.GetComponent<AgentEvent>();
            mapping.target = agentEvent;
            agentEvent.AddAttendee(agent);
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

        public override bool PreMatch(OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                      MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            // The EntityType being created need match with the ICMT's EntityType TypeGroup Target
            List<TypeGroup> typeGroups = inputConditionMappingType.EntityTypeGroupings();

            if (outputChange.entityType.InAllTypeGroups(typeGroups))
                return true;
            return false;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // All matching needed is done in PreMatch (since the specific EntityType being created needs to be specified)            
            return true;
        }
    }
}
