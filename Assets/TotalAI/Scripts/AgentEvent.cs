using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    public class AgentEvent : Entity
    {
        // All Events start in Waiting
        // Once the number of attendees reaches minAttendees - state becomes Starting
        // Once all attendees are ReadyToStart - state becomes Running
        // Once all attendees are ReadyToEnd - state becomes Ending
        // Once all attendees are Ended - the event is destroyed

        [Serializable]
        public class Location
        {
            public List<Transform> nodes;
        }

        public enum State { Waiting, Starting, Running, Ending }
        public enum AttendeeState { Waiting, ReadyToStart, ReadyToEnd, Ended }
        
        [Header("Runtime Fields")]
        public State state;
        public Agent creator;

        [Serializable]
        public class Attendee
        {
            public Agent agent;
            public AttendeeState state;
        }
        public List<Attendee> attendees;

        [HideInInspector]
        public AgentEventType agentEventType;

        private List<Agent> travellingTo;

        private float startTime;
        private float timeLimit;
        private TimeManager timeManager;

        private new void Awake()
        {
            base.Awake();
            timeManager = GameObject.Find("TimeManager").GetComponent<TimeManager>();
            agentEventType = (AgentEventType)entityType;
            travellingTo = new List<Agent>();

            // TODO: Should AgentEvent have all of the Entity stuff?
            ResetEntity();
        }

        public bool TimeLimitExpired()
        {
            return timeLimit != -1f && Time.time - timeLimit > startTime;
        }

        public void NotifyTravellingTo(Agent agent)
        {
            if (!travellingTo.Contains(agent))
                travellingTo.Add(agent);
        }

        public void SetAttendeeState(Agent agent, AttendeeState attendeeState)
        {
            foreach (Attendee attendee in attendees)
            {
                if (attendee.agent == agent)
                    attendee.state = attendeeState;
            }

            // See if all attendees are now ready to move event to next state
            if (CanMoveToNextState())
                MoveToNextState();
        }

        public override void DestroySelf(Agent agent, float delay = 0f)
        {
            Destroy(gameObject, delay);
        }

        public bool CanJoin(Agent agent)
        {
            MemoryType.EntityInfo entityInfo = creator.memoryType.KnownEntity(creator, agent);

            // TODO: Figure out if non-faction AgentEvents should be allowed?
            return state == State.Waiting && (!agentEventType.onlyFactionMembers || creator.faction == agent.faction) &&
                   ((agent.faction == null && creator.faction == null) || agent.faction.AllowEventType(agent.roleTypes, agentEventType, false)) &&
                   (creator == agent || (AttendeeCount() < agentEventType.maxAttendees &&
                   entityInfo != null && entityInfo.rLevel >= agentEventType.rLevelMin));
        }

        // Adds in creator in case creator hasn't joined yet
        private int AttendeeCount()
        {
            foreach (Attendee attendee in attendees)
            {
                if (attendee.agent == creator)
                    return attendees.Count;
            }            
            return attendees.Count + 1;
        }

        // IncludeGoTo will check to see if any agents are currently traveling to the event
        public bool HasMinAttendees(bool IncludeGoTo = false)
        {
            if (!IncludeGoTo)
            {
                return AttendeeCount() >= agentEventType.minAttendees;
            }

            return AttendeeCount() + travellingTo.Count >= agentEventType.minAttendees;
        }

        public AttendeeState GetAttendeeState(Agent agent)
        {
            foreach (Attendee attendee in attendees)
            {
                if (attendee.agent == agent)
                    return attendee.state;
            }
            Debug.LogError("Trying to get attendee state and agent (" + agent.name + ") is not an Attendee!");
            return AttendeeState.Ended;
        }
        public void AddAttendee(Agent agent, float delay)
        {
            StartCoroutine(AddAttendeeCoroutine(agent, delay));
        }

        private IEnumerator AddAttendeeCoroutine(Agent agent, float delay)
        {
            yield return new WaitForSeconds(delay);
            AddAttendee(agent);
        }

        public void AddAttendee(Agent agent)
        {
            travellingTo.Remove(agent);
            attendees.Add(new Attendee()
            {
                agent = agent,
                state = AttendeeState.Waiting
            });
            
            // Start up event for all attendees
            if (attendees.Count == agentEventType.minAttendees && state == State.Waiting)
            {
                StartCoroutine(StartEvent());
            }
        }

        private IEnumerator StartEvent()
        {
            if (agentEventType.timeToWaitAfterMin > 0)
                yield return new WaitForSeconds(agentEventType.timeToWaitAfterMin);

            state = State.Starting;
            startTime = Time.time;
            if (agentEventType.runningTimeLimit == null)
                timeLimit = -1;
            else
                timeLimit = agentEventType.runningTimeLimit.EvalRandom() * timeManager.RealTimeSecondsPerGameMinute();

            // Notify Attendees that event is starting - set all BT event params here since we now know all attendees
            // TODO: Handle agents joining event after start?
            foreach (Attendee attendee in attendees)
            {
                attendee.agent.EventStarting(this);
            }
        }

        public List<RoleType> GetRoleTypes(Agent agent)
        {
            switch (agentEventType.roleMappingType)
            {
                case AgentEventType.RoleMappingType.CreatorAttendee:
                    if (agent == creator)
                        return agentEventType.attendeeRoleMappings.Find(x => x.forCreator).roleTypes;
                    return agentEventType.attendeeRoleMappings.Find(x => !x.forCreator).roleTypes;
                case AgentEventType.RoleMappingType.JoinOrder:
                    int index = attendees.FindIndex(x => x.agent == agent);
                    if (index != -1)
                    {
                        List<AgentEventType.AttendeeRoleMapping> orderMatches;
                        orderMatches = agentEventType.attendeeRoleMappings.FindAll(x => x.mappingJoinOrders.Contains(index));
                        return orderMatches.SelectMany(x => x.roleTypes).ToList();
                    }
                    break;
                case AgentEventType.RoleMappingType.HasRoleType:
                    List<AgentEventType.AttendeeRoleMapping> roleMatches;
                    roleMatches = agentEventType.attendeeRoleMappings.FindAll(x => agent.ActiveRoles().Keys.Any(y => x.mappingRoleTypes.Contains(y)));
                    return roleMatches.SelectMany(x => x.roleTypes).ToList();
            }
            Debug.Log(agent.name + ": AgentEvent.GetRoleTypes for " + name + " unable to find Any RoleTypes for Agent.");
            return null;
        }

        public void RemoveAttendee(Agent agent)
        {
            for (int i = 0; i < attendees.Count; i++)
            {
                if (attendees[i].agent == agent)
                {
                    attendees.RemoveAt(i);
                    break;
                }
            }

            if (attendees.Count == 0 && gameObject != null)
            {
                // All attendees were removed - the event must have timed out
                DestroySelf(agent);
            }
            else if ((agentEventType.endWhenLessThanMin && attendees.Count < agentEventType.minAttendees) ||
                     (agentEventType.endWhenCreatorQuits && agent == creator))
            {
                // Kills the event
                timeLimit = 0.01f;
            }
        }

        // state is current state so for example if all attendees are in ReadyToStart - state can be moved fro Starting to Running
        // and if state is Ending and all attendees have ended - AgentEvent can be destroyed
        private bool CanMoveToNextState()
        {
            switch (state)
            {
                case State.Waiting:
                    break;
                case State.Starting:
                    if (AllAttendeesInState(AttendeeState.ReadyToStart))
                        return true;
                    break;
                case State.Running:
                    if (AllAttendeesInState(AttendeeState.ReadyToEnd))
                        return true;
                    break;
                case State.Ending:
                    if (AllAttendeesInState(AttendeeState.Ended))
                        return true;
                    break;
                default:
                    break;
            }

            return false;
        }

        private bool AllAttendeesInState(AttendeeState attendeeState)
        {
            foreach (Attendee attendee in attendees)
            {
                if (attendee.state != attendeeState)
                    return false;
            }
            return true;
        }

        private void MoveToNextState()
        {
            if (state == State.Ending)
            {
                // Event is over (All agents have ended) - destroy it
                DestroySelf(null);
            }
            else
            {
                state = state + 1;

                if (state == State.Running)
                {
                    AudioSource audioSource = GetComponent<AudioSource>();
                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.loop = false;
                        audioSource.Stop();
                    }
                }
            }
        }
    }
}
