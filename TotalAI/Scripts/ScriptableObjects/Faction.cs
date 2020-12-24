using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "Faction", menuName = "Total AI/Faction", order = 1)]
    public class Faction : ScriptableObject
    {
        [System.Serializable]
        public class EventTypePermission
        {
            public RoleType roleType;
            public AgentEventType agentEventType;
            public bool canBeCreator;
            public bool canBeAttendee;
        }
        public Color color;

        [Header("Other Factions Default R Levels")]
        public List<Faction> otherFaction;
        // When an agent of one faction first meets an agent of another faction get rLevel from this
        public List<float> defaultRLevels;

        [Header("Agent Event Info")]
        public List<EventTypePermission> eventTypePermissions;
        
        private List<Agent> agents;

        private void OnEnable()
        {
            agents = new List<Agent>();
        }

        public void SetupAgent(Agent agent)
        {
            // Needed to clear out List after playing for Editor
            if (!Application.isPlaying && agents != null && agents.Contains(agent))
            {
                OnEnable();
            }

            if (agent.faction != null && agent.faction == this)
                agents.Add(agent);
        }

        public List<Agent> GetAllAgents()
        {
            return agents;
        }

        public float GetDefaultRLevel(Faction faction)
        {
            int otherFactionIndex = otherFaction.IndexOf(faction);
            if (otherFactionIndex != -1)
                return defaultRLevels[otherFactionIndex];
            return 50f;
        }

        public bool AllowEventType(List<RoleType> roleTypes, AgentEventType agentEventType, bool forCreating)
        {
            foreach (EventTypePermission eventTypePermission in eventTypePermissions)
            {
                if (agentEventType == eventTypePermission.agentEventType && roleTypes.Contains(eventTypePermission.roleType))
                {
                    if (forCreating)
                    {
                        if (eventTypePermission.canBeCreator)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        if (eventTypePermission.canBeAttendee)
                            return true;
                        else
                            return false;
                    }
                }
            }
            // If AgentEventType/Role combo is not in permissions then don't allow it
            Debug.Log("Unable to find explicit agent event type permission in faction - denying permission to create event.");
            return false;
        }

        // A member calls this when they change a faction drive level
        public void UpdateOtherMembersDriveLevel(Agent updatingAgent, DriveType driveType, float amount)
        {
            foreach (Agent agent in agents)
            {
                if (updatingAgent != agent)
                    agent.drives[driveType].ChangeFactionDriveLevel(amount);
            }
        }

    }
}