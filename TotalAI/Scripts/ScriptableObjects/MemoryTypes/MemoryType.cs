using System.Collections.Generic;
using UnityEngine;
using System;

namespace TotalAI
{
    public abstract class MemoryType : ScriptableObject
    {
        [Serializable]
        public class EntityInfo
        {
            public Entity entity;
            public Vector3 lastPos;
            public float distance;
            public float rLevel;       // The relationship level for an agent with this agent
            public float lastUpdated;  // time in seconds since the start of the game: (Time.time - lastUpdated) to get time since updated

            public EntityInfo(Entity entity, Vector3 lastPos, float distance, float rLevel, float lastUpdated)
            {
                this.entity = entity;
                this.lastPos = lastPos;
                this.distance = distance;
                this.rLevel = rLevel;
                this.lastUpdated = lastUpdated;
            }

        }

        public enum EntityTypesEnum { EntityType, WorldObjectType, AgentType, AgentEventType }

        public abstract void Setup(Agent agent);
        public abstract void UpdateShortTermMemory(Agent agent);
        public abstract void AddEntities(Agent agent, Entity[] entities, int numEntities);

        public abstract void ChangeRLevel(Agent agent, Agent otherAgent, float amount);

        // Search Methods
        public abstract List<EntityInfo> GetShortTermMemory(Agent agent);
        public abstract bool InShortTermMemory(Agent agent, Agent otherAgent);
        public abstract Vector3 GetKnownPosition(Agent agent, Entity entity);
        public abstract EntityInfo KnownEntity(Agent agent, Entity entity);
        public abstract List<EntityInfo> GetKnownEntities(Agent agent, float radius = -1, bool detectInventory = false);
        public abstract List<EntityInfo> GetKnownEntities(Agent agent, EntityTypesEnum entityTypesEnum, float radius = -1, bool detectInventory = false);
        public abstract List<EntityInfo> GetKnownEntities(Agent agent, EntityType entityType, float radius = -1, bool detectInventory = false);
        public abstract List<EntityInfo> GetKnownEntities(Agent agent, List<EntityType> entityTypes, float radius = -1, bool detectInventory = false);
        public abstract List<EntityType> GetKnownEntityTypes(Agent agent, bool detectInventory = false);

        // Remove Memories over time
        public abstract int Decay(Agent agent);

        // Called by Entity Disabled Event - remove this entity from memory
        public abstract void KnownEntityDisabled(Entity entity);

        // String represention of known entities - used in Game GUI (GUIManager)
        public abstract string KnownToString(Agent agent);

        // For AgentView Editor Window
        public abstract string[,] EditorAgentViewData(Agent agent, out List<UnityEngine.Object> objects);
        public abstract string[,] PlayerAgentViewData(Agent agent, out List<UnityEngine.Object> objects);
    }
}