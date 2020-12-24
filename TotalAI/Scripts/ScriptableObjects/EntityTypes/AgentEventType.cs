using System;
using System.Collections.Generic;
using UnityEngine;


namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentEventType", menuName = "Total AI/Entity Types/Agent Event Type", order = 0)]
    public class AgentEventType : EntityType
    {
        public float maxTimeToWait;
        public float timeToWaitAfterMin;
        [Tooltip("Event won't be cancelled if time to wait expires but there are enough agents travelling to the event.")]
        public bool includeGoToAgents;
        
        public bool onlyFactionMembers;
        public float rLevelMin;  // Minimum rLevel to join event - based on creator’s rLevels
        public int minAttendees;
        public int maxAttendees;

        public enum RoleMappingType { CreatorAttendee, JoinOrder, HasRoleType };
        public RoleMappingType roleMappingType;

        [Serializable]
        public class AttendeeRoleMapping
        {
            public bool forCreator;
            public List<int> mappingJoinOrders;
            public List<RoleType> mappingRoleTypes;
            public List<RoleType> roleTypes;
        }
        public List<AttendeeRoleMapping> attendeeRoleMappings;

        public AudioClip soundForWaiting;
        public bool loop;

        [Header("Event time limit in game hours.  Leave at None to have no limit.")]
        public MinMaxCurve runningTimeLimit;

        [Header("Should event end when the event creator quits?")]
        public bool endWhenCreatorQuits;

        [Header("Should event end when # of attendees drops below min # of attendees?")]
        public bool endWhenLessThanMin;

        public bool CanCreate(Agent agent)
        {
            // TODO: Should there be a non faction check?
            if (agent.faction == null || agent.faction.AllowEventType(agent.roleTypes, this, true))
                return true;
            return false;
        }

        public override GameObject CreateEntity(int prefabVariantIndex, Vector3 position, Quaternion rotation, Vector3 scale, Entity creator)
        {
            // This appears to work - During runtime the prefab must be saved somewhere - does not change assets
            GameObject prefab = prefabVariants[prefabVariantIndex];
            prefab.SetActive(false);
            GameObject agentEventGameObject = Instantiate(prefab, position, rotation);
            AgentEvent agentEvent = agentEventGameObject.GetComponent<AgentEvent>();

            agentEvent.state = AgentEvent.State.Waiting;

            // Set entityType - this allows one prefab to be used for multiple AgentEventTypes
            agentEvent.entityType = this;
            agentEvent.agentEventType = this;

            if (creator != null)
                agentEvent.creator = (Agent)creator;
            agentEvent.attendees = new List<AgentEvent.Attendee>();

            // Create the trigger collider
            //SphereCollider sc = agentEventGameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
            //sc.isTrigger = true;
            //sc.radius = detectionRadius;

            // Set Waiting Sound
            AudioSource audioSource = agentEvent.GetComponent<AudioSource>();
            if (audioSource != null && soundForWaiting != null)
            {
                audioSource.clip = soundForWaiting;
                audioSource.loop = loop;
                audioSource.playOnAwake = true;
            }
            else if (audioSource == null && soundForWaiting != null)
            {
                Debug.LogError(name + " has waiting sound set but the AgentEvent " + agentEvent.name + " is missing an Audio Sorce.");
            }
            agentEventGameObject.SetActive(true);
            prefab.SetActive(true);

            return agentEventGameObject;
        }

        // AgentEvent can't be in an Entity's inventory
        public override Entity CreateEntityInInventory(Entity entity, int prefabVariantIndex, InventorySlot inventorySlot)
        {
            Debug.LogError("Trying to create an AgentEventType Entity inside an inventory.");
            return null;
        }
    }
}

