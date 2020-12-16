using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "EntityExistsICT", menuName = "Total AI/Input Condition Types/Entity Exists", order = 0)]
    public class EntityExistsICT : InputConditionType
    {
        public enum Scope { AgentCreated, FactionOwned, AllEntities }

        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Entity Exists</b>: How many?  What is the scope?  Should it be from the Agent's memory or be the current ground truth?",
                usesEntityType = true,
                mostRestrictiveEntityType = typeof(EntityType),
                usesMinMax = true,
                minLabel = "Min Number Entities",
                maxLabel = "Max Number Entities",
                usesBoolValue = true,
                boolLabel = "Use Agent's Knowledge",
                usesEnumValue = true,
                enumType = typeof(Scope)
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            // If boolValue is true - use the agent's memory - else use the global truth
            if (inputCondition.boolValue)
            {
                List<MemoryType.EntityInfo> entityInfos = agent.memoryType.GetKnownEntities(agent, inputCondition.entityType);
                
                if (entityInfos.Count >= inputCondition.min && entityInfos.Count <= inputCondition.max)
                    return true;
            }
            else
            {
                // TODO: Some kind of game manager to cache world state                
                Entity[] entities = FindObjectsOfType<Entity>();

                int count = 0;
                foreach (Entity entity in entities)
                {
                    if (entity.entityType == inputCondition.entityType)
                        ++count;
                }

                if (count >= inputCondition.min && count <= inputCondition.max)
                    return true;
            }

            return false;
        }
    }
}
