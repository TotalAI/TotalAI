using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "StartAgentEventBT", menuName = "Total AI/Behavior Types/Start Agent Event", order = 0)]
    public class StartAgentEventBT : BehaviorType
    {
        // All Events start in Waiting - state is kept in AgentEvent
        // Once the number of attendees reaches minAttendees - state becomes Starting
        // Once all attendees are ReadyToStart - state becomes Running
        // Once all attendees are ReadyToEnd - state becomes Ending
        // Once all attendees are Ended - the event is destroyed
        
        public float maxRotationDegreesDelta = 5f;
        public float distanceTolerance = 1f;
        public float rotationTolerance = 1f;
        
        public class Context
        {
            public List<Selector> attributeSelectors;
            public float startTime;
            public AgentEvent agentEvent;
            public Vector3 targetPosition;
            public float movementSpeed;
            public string boolParamName;
            public bool startedMoving;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(MinMaxFloatAT), typeof(EnumStringAT) },
                valueDescriptions = new string[] { "Movement Speed", "Animator Bool Param Name" }
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }
            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.agentEvent = (AgentEvent)behaviorContext.target;
            context.startTime = Time.time;
            context.startedMoving = false;
            context.targetPosition = Vector3.positiveInfinity;

            context.movementSpeed = context.attributeSelectors[0].GetFloatValue(agent, agent.decider.CurrentMapping);
            context.boolParamName = context.attributeSelectors[1].GetEnumValue<string>(agent, agent.decider.CurrentMapping);
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            agent.movementType.SetStopped(agent, true);
            agent.animationType.SetBool(agent, context.boolParamName, true);
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            switch (context.agentEvent.state)
            {
                case AgentEvent.State.Waiting:
                    // If its past wait time - not enough agents joined event so quit
                    float waitTime = context.agentEvent.agentEventType.maxTimeToWait;
                    bool hasMin = context.agentEvent.HasMinAttendees(context.agentEvent.agentEventType.includeGoToAgents);

                    if (hasMin)
                    {
                        waitTime += context.agentEvent.agentEventType.timeToWaitAfterMin;
                    }

                    if (!hasMin && Time.time - context.startTime > waitTime)
                    {
                        Debug.Log(agent.name + ": Quitting Agent Event (" + context.agentEvent.agentEventType.name +
                                  ") - not enough attendees within wait time: " + waitTime + " seconds");

                        agent.animationType.SetBool(agent, context.boolParamName, false);

                        // This tells decider that this action was interrupted from the BehaviorType
                        agent.behavior.InterruptBehavior(true);

                        // This will remove the agent from the agent event
                        InterruptBehavior(agent);
                    }
                    break;
                case AgentEvent.State.Starting:
                    // First time since Agent Event state moved to Starting
                    if (!context.startedMoving)
                    {
                        Debug.Log(agent.name + ": 1st time in Starting for Agent Event (" + context.agentEvent.agentEventType.name + ")");

                        agent.animationType.SetBool(agent, context.boolParamName, false);

                        // Move agent into position and then let the event know this agent is ready to start event
                        // TODO: Is this needed anymore?
                        context.targetPosition = agent.transform.position;
                        agent.behavior.SetTargetPosition(context.targetPosition);
                        //agent.movementType.SetStopped(agent, false);
                        //agent.movementType.SetSpeed(agent, context.movementSpeed);
                        //agent.movementType.SetDestination(agent, context.targetPosition);
                        context.startedMoving = true;
                    }
                    else
                    {
                        Debug.Log(agent.name + ": In Starting for Agent Event (" + context.agentEvent.agentEventType.name + ")");
                        // TODO: Move movement methods into Behavior base class or a static utility class
                        float distanceToTargetPosition = Vector3.Distance(agent.transform.position, context.targetPosition);

                        // TODO: Add in lookAt - also a way to bail out or switch locations if agent can't get to targetPosition
                        if (distanceToTargetPosition <= distanceTolerance)
                        {
                            Debug.Log(agent.name + ": Reached target position for Agent Event (" + context.agentEvent.agentEventType.name + ")");
                            context.agentEvent.SetAttendeeState(agent, AgentEvent.AttendeeState.ReadyToStart);
                        }
                    }
                    break;
                /*
                case AgentEvent.State.Running:
                    // First time in Running state
                    if (context.loopStartTime == -1f)
                    {
                        Debug.Log(agent.name + ": 1st time in Running for Agent Event (" + context.agentEvent.agentEventType.name + ")");
                        context.loopStartTime = Time.time;
                        if (context.animParamNames.Count > context.animParamIndex)
                        {
                            agent.animationType.SetTrigger(agent, context.animParamNames[context.animParamIndex]);
                            //context.animParamIndex++;
                        }
                    }
                    else if (Time.time - context.loopStartTime > context.waitTimes[1] &&
                             context.currentLoopCount != context.agentEvent.agentEventType.numRunningLoops)
                    {
                        // Loop through animations - when done let event know agent is ready to end event
                        context.loopStartTime = Time.time;
                        context.currentLoopCount++;

                        Debug.Log(agent.name + ": Agent Event Loop Increased - currently at " + context.currentLoopCount);

                        if (context.currentLoopCount == context.agentEvent.agentEventType.numRunningLoops)
                        {
                            context.agentEvent.SetAttendeeState(agent, AgentEvent.AttendeeState.ReadyToEnd);
                        }
                        else if (context.animParamNames.Count > context.animParamIndex)
                        {
                            agent.animationType.SetTrigger(agent, context.animParamNames[context.animParamIndex]);
                            //context.animParamIndex++;
                        }
                    }
                    
                    break;
                case AgentEvent.State.Ending:
                    // Event is over
                    context.agentEvent.SetAttendeeState(agent, AgentEvent.AttendeeState.Ended);
                    Debug.Log(agent.name + ": Agent Event in Ending - set attendeeState to Ended");
                    //TODO: Should finish be forced here?

                    break;
                */
            }
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Agent Event is only finished when the attendee state is Ended or AgentEvent was already destroyed - else it was interrupted
            if (context.agentEvent == null || context.agentEvent.state == AgentEvent.State.Running)
            {
                agent.movementType.SetStopped(agent, true);
                return true;
            }
            //if (context.agentEvent == null || context.agentEvent.GetAttendeeState(agent) == AgentEvent.AttendeeState.Ended)
            //    return true;

            return false;
        }

        public override void InterruptBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Interrupted this Behavior in the middle of it
            // Get back to grounded state
            agent.animationType.SetBool(agent, context.boolParamName, false);
            agent.movementType.SetStopped(agent, true);
            context.agentEvent.RemoveAttendee(agent);
        }

        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            return -1f;
        }
    }
}
