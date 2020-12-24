using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "BaseMemoryType", menuName = "Total AI/Memory Types/Base", order = 0)]
    public class BaseMemoryType : MemoryType
    {
        [Header("How long in realtime seconds to keep EntityInfo in short term memory?")]
        public float shortTermMemoryTime = 30f;

        [Header("If no faction RLevel setting - what RLevel to set for same AgentTypes?")]
        public float defaultSameAgentTypeRLevel = 50f;

        [Header("If no faction RLevel setting - what RLevel to set for different AgentTypes?")]
        public float defaultDiffAgentTypeRLevel = 10f;

        // Stores recently detected Agents
        private Dictionary<Agent, List<EntityInfo>> shortTermMemory;

        // Main memory storage for all Entities ever detected
        private Dictionary<Agent, Dictionary<Entity, EntityInfo>> knownEntities;

        private void OnEnable()
        {
            shortTermMemory = new Dictionary<Agent, List<EntityInfo>>();
            knownEntities = new Dictionary<Agent, Dictionary<Entity, EntityInfo>>();
        }

        public override void Setup(Agent agent)
        {
            shortTermMemory[agent] = new List<EntityInfo>();
            knownEntities[agent] = new Dictionary<Entity, EntityInfo>();
        }

        // Called by Entity Disabled Event - remove this entity from known, withinKnown, and near lists if possible
        public override void KnownEntityDisabled(Entity entity)
        {
            entity.EntityDisabledEvent -= KnownEntityDisabled;
            foreach (Agent agent in knownEntities.Keys)
            {
                if (knownEntities[agent].TryGetValue(entity, out EntityInfo entityInfo))
                {
                    knownEntities[agent].Remove(entity);
                    shortTermMemory[agent].Remove(entityInfo);
                }
            }
        }

        public override void UpdateShortTermMemory(Agent agent)
        {
            shortTermMemory[agent].RemoveAll(x => Time.time - x.lastUpdated > shortTermMemoryTime);
        }

        public override void AddEntities(Agent agent, Entity[] entities, int numEntities)
        {
            for (int i = 0; i < numEntities; i++)
            {
                //GameObject gameObject = entities[i].gameObject;
                Entity entity = entities[i];
                Agent entityAgent = entity as Agent;
                float distance = Vector3.Distance(agent.transform.position, entity.transform.position);

                // Add to knownEntities - will change info if entity is already known
                EntityInfo entityInfo = null;
                if (knownEntities[agent].TryGetValue(entity, out entityInfo))
                {
                    entityInfo.lastPos = entity.transform.position;
                    entityInfo.distance = distance;
                    entityInfo.lastUpdated = Time.time;

                    if (entityAgent != null && !shortTermMemory[agent].Contains(entityInfo))
                        shortTermMemory[agent].Add(entityInfo);
                }
                else
                {
                    // New entity discovered
                    entityInfo =
                        new EntityInfo(entity, entity.transform.position, distance, -1, Time.time);
                    knownEntities[agent][entity] = entityInfo;

                    if (entityAgent != null)
                        shortTermMemory[agent].Add(entityInfo);

                    // Subscribe to new entity's disabled event to know when to remove entity from known list
                    entity.EntityDisabledEvent += KnownEntityDisabled;
                }

                // Set R value to default if it has not been set - only for agent entities
                if (entityAgent != null && entityInfo.rLevel == -1)
                {
                    if (agent.faction != null && entityAgent.faction != null)
                        entityInfo.rLevel = agent.faction.GetDefaultRLevel(entityAgent.faction);
                    else if (entity.entityType == agent.agentType)
                        entityInfo.rLevel = defaultSameAgentTypeRLevel;
                    else
                        entityInfo.rLevel = defaultDiffAgentTypeRLevel;
                }
            }
        }

        public override void ChangeRLevel(Agent agent, Agent otherAgent, float amount)
        {
            EntityInfo entityInfo = null;
            if (knownEntities[agent].TryGetValue(otherAgent, out entityInfo))
            {
                entityInfo.rLevel += amount;
            }
            else
            {
                Debug.LogError("Trying to update the R Level of an agent (" + otherAgent.name + ") that the agent (" + agent.name + ") doesn't know!");
            }
        }

        public override List<EntityInfo> GetShortTermMemory(Agent agent)
        {
            return shortTermMemory[agent];
        }

        public override bool InShortTermMemory(Agent agent, Agent otherAgent)
        {
            return shortTermMemory[agent].Any(x => x.entity == otherAgent);
        }

        // Returns null if the entity is not known
        public override EntityInfo KnownEntity(Agent agent, Entity entity)
        {
            if (entity == null || entity.gameObject == null)
                return null;

            knownEntities[agent].TryGetValue(entity, out EntityInfo entityInfo);
            return entityInfo;
        }

        // Returns PositiveInfinity if the entity is not known
        public override Vector3 GetKnownPosition(Agent agent, Entity entity)
        {
            if (entity == null || entity.gameObject == null)
                return Vector3.positiveInfinity;

            knownEntities[agent].TryGetValue(entity, out EntityInfo entityInfo);
            if (entityInfo == null)
                return Vector3.positiveInfinity;
            return entityInfo.lastPos;
        }

        // TODO: DRY up the GetKnownEntities methods
        public override List<EntityInfo> GetKnownEntities(Agent agent, float radius, bool detectInventory)
        {
            List<EntityInfo> results = new List<EntityInfo>();
            if (radius <= 0)
            {
                foreach (EntityInfo entityInfo in knownEntities[agent].Values)
                {
                    if (detectInventory || entityInfo.entity.inEntityInventory == null)
                    {
                        results.Add(entityInfo);
                    }
                }
            }
            else
            {
                foreach (EntityInfo entityInfo in knownEntities[agent].Values)
                {
                    if ((detectInventory || entityInfo.entity.inEntityInventory == null) &&
                        Vector3.Distance(entityInfo.lastPos, agent.transform.position) < radius)
                    {
                        results.Add(entityInfo);
                    }
                }
            }
            return results;
        }

        public override List<EntityInfo> GetKnownEntities(Agent agent, EntityTypesEnum entityTypesEnum, float radius, bool detectInventory)
        {
            List<EntityInfo> results = new List<EntityInfo>();
            if (radius <= 0)
            {
                foreach (EntityInfo entityKnowledgeInfo in knownEntities[agent].Values)
                {
                    if (MatchesType(entityTypesEnum, entityKnowledgeInfo.entity.entityType) &&
                        (detectInventory || entityKnowledgeInfo.entity.inEntityInventory == null))
                    {
                        results.Add(entityKnowledgeInfo);
                    }
                }
            }
            else
            {
                foreach (EntityInfo entityKnowledgeInfo in knownEntities[agent].Values)
                {
                    if (MatchesType(entityTypesEnum, entityKnowledgeInfo.entity.entityType) &&
                        (detectInventory || entityKnowledgeInfo.entity.inEntityInventory == null) &&
                        Vector3.Distance(entityKnowledgeInfo.lastPos, agent.transform.position) < radius)
                    {
                        results.Add(entityKnowledgeInfo);
                    }
                }
            }
            return results;
        }

        public override List<EntityInfo> GetKnownEntities(Agent agent, EntityType entityType, float radius, bool detectInventory)
        {
            List<EntityInfo> results = new List<EntityInfo>();
            if (radius <= 0)
            {
                foreach (EntityInfo entityKnowledgeInfo in knownEntities[agent].Values)
                {
                    if (entityKnowledgeInfo.entity.entityType == entityType &&
                        (detectInventory || entityKnowledgeInfo.entity.inEntityInventory == null))
                    {
                        results.Add(entityKnowledgeInfo);
                    }
                }
            }
            else
            {
                foreach (EntityInfo entityKnowledgeInfo in knownEntities[agent].Values)
                {
                    if (entityKnowledgeInfo.entity.entityType == entityType &&
                        (detectInventory || entityKnowledgeInfo.entity.inEntityInventory == null) &&
                        Vector3.Distance(entityKnowledgeInfo.lastPos, agent.transform.position) < radius)
                    {
                        results.Add(entityKnowledgeInfo);
                    }
                }
            }
            return results;
        }

        public override List<EntityInfo> GetKnownEntities(Agent agent, List<EntityType> entityTypes, float radius, bool detectInventory)
        {
            List<EntityInfo> results = new List<EntityInfo>();
            foreach (EntityType entityType in entityTypes)
            {
                results.AddRange(GetKnownEntities(agent, entityType, radius, detectInventory));
            }
            return results;
        }

        public override List<EntityType> GetKnownEntityTypes(Agent agent, bool detectInventory = false)
        {
            if (!detectInventory)
                return knownEntities[agent].Keys.Select(x => x.entityType).Distinct().ToList();
            return GetKnownEntities(agent, -1, true).Select(x => x.entity.entityType).Distinct().ToList();
        }

        public override int Decay(Agent agent)
        {
            return 0;
        }

        private bool MatchesType(EntityTypesEnum entityTypesEnum, EntityType entityType)
        {
            // General Type level checks
            switch (entityTypesEnum)
            {
                case EntityTypesEnum.EntityType:
                    return true;
                case EntityTypesEnum.AgentType:
                    if (entityType is AgentType) return true;
                    break;
                case EntityTypesEnum.WorldObjectType:
                    if (entityType is WorldObjectType) return true;
                    break;
                case EntityTypesEnum.AgentEventType:
                    if (entityType is AgentEventType) return true;
                    break;
            }
            return false;
        }

        public override string KnownToString(Agent agent)
        {
            string result = "";

            foreach (EntityInfo entityInfo in knownEntities[agent].Values)
            {
                result += entityInfo.entity.name + " (" + entityInfo.entity.entityType.name + ") - D = " + entityInfo.distance;
                if (entityInfo.entity.entityType is AgentType)
                    result += " - R = " + entityInfo.rLevel;
                result += "\n";
            }

            return result;
        }

        public override string[,] EditorAgentViewData(Agent agent, out List<UnityEngine.Object> objects)
        {
            objects = new List<UnityEngine.Object>();
            string[,] data = new string[4, 2];
            objects.Add(this);
            objects.Add(null);
            objects.Add(null);
            objects.Add(null);

            data[0, 0] = name;
            data[1, 0] = "Seconds in Short Term Memory";
            data[1, 1] = shortTermMemoryTime.ToString();
            data[2, 0] = "Default Same AgentType RLevel";
            data[2, 1] = defaultSameAgentTypeRLevel.ToString();
            data[3, 0] = "Default Diff AgentType RLevel";
            data[3, 1] = defaultDiffAgentTypeRLevel.ToString();
            
            return data;
        }

        public override string[,] PlayerAgentViewData(Agent agent, out List<UnityEngine.Object> objects)
        {
            objects = new List<UnityEngine.Object>();
            int numRows = shortTermMemory[agent].Where(x => x.entity != null).Count() +
                          knownEntities[agent].Where(x => x.Key != null).Count() + 3;
            string[,] data = new string[numRows, 4];

            objects.Add(null);
            data[0, 0] = "Agent Short Term";
            int row = 1;
            foreach (EntityInfo entityInfo in shortTermMemory[agent])
            {
                if (entityInfo.entity == null)
                    continue;

                objects.Add(entityInfo.entity);
                data[row, 0] = entityInfo.entity.name;
                data[row, 1] = Mathf.Round(entityInfo.distance).ToString();
                data[row, 2] = entityInfo.rLevel.ToString();
                data[row, 3] = Mathf.Round(Time.time - entityInfo.lastUpdated).ToString();
                ++row;
            }
            objects.Add(null);
            ++row;
            objects.Add(null);
            data[row, 0] = "Long Term";
            ++row;
            foreach (KeyValuePair<Entity, EntityInfo> entityInfo in knownEntities[agent])
            {
                if (entityInfo.Key == null)
                    continue;

                objects.Add(entityInfo.Key);
                data[row, 0] = entityInfo.Key.name;
                data[row, 1] = Mathf.Round(entityInfo.Value.distance).ToString();
                data[row, 2] = entityInfo.Value.rLevel.ToString();
                data[row, 3] = Mathf.Round(Time.time - entityInfo.Value.lastUpdated).ToString();
                ++row;
            }

            return data;
        }
    }
}

