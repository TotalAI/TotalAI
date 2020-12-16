using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WaitForDriveBT", menuName = "Total AI/Behavior Types/Wait For Drive", order = 0)]
    public class WaitForDriveBT : BehaviorType
    {
        [Header("End wait when current mapping's drive level gets above or below Drive Level To Wait For")]
        [Range(0,100)]
        public float driveLevelThreshold = 0;

        public enum EqualityType { LessThanOrEqual, GreaterThanOrEqual }
        [Header("Does the drive level need to be above or below threshold to end waiting?")]
        public EqualityType equalityType;

        [Header("Wait on Target's Drive Level instead of this agent")]
        public bool useTargetsDriveLevel;

        public class Context
        {
            public DriveType driveTypeToWaitFor;
            public Agent targetAgent;
            public List<Selector> attributeSelectors;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(EnumStringAT) },
                valueDescriptions = new string[] { "Animation Trigger Param Name" }
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            DriveType driveType = null;
            Agent targetAgent = null;
            if (useTargetsDriveLevel)
            {
                if (behaviorContext.target == null || behaviorContext.target.GetComponent<Agent>() == null)
                {
                    Debug.LogError("WaitForDriveBehavior using targets drive is missing target or target is not an Agent.");
                }
                else
                {
                    targetAgent = behaviorContext.target.GetComponent<Agent>();
                    driveType = (DriveType)agent.decider.CurrentMapping.mappingType.inputConditions[0].levelType;
                }
            }
            else
            {
                targetAgent = agent;
                driveType = agent.decider.CurrentDriveType;
            }

            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }

            context.attributeSelectors = behaviorContext.attributeSelectors;
            context.driveTypeToWaitFor = driveType;
            context.targetAgent = targetAgent;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            Selector attributeSelector = context.attributeSelectors[0];
            string triggerName = attributeSelector.GetEnumValue<string>(agent, agent.decider.CurrentMapping);
            agent.animationType.SetTrigger(agent, triggerName);
        }

        public override void UpdateBehavior(Agent agent)
        {
            //Debug.Log("Animation: " + animator.GetCurrentAnimatorStateInfo(0).length + " - " + animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            //Debug.Log(animator.GetNextAnimatorStateInfo(0).IsName(groundState));
            /*
            WaitForDriveBehaviorTypeContext context = (WaitForDriveBehaviorTypeContext)behaviorTypeContext;

            // Wait until drive level is at or above driveTypeToWaitFor - then do animation
            bool levelReached = false;
            if (equalityType == EqualityType.GreaterThanOrEqual)
                levelReached = context.targetAgent.drives[context.driveTypeToWaitFor].GetLevel() >= driveLevelThreshold;
            else
                levelReached = context.targetAgent.drives[context.driveTypeToWaitFor].GetLevel() <= driveLevelThreshold;

            if ((context.driveTypeToWaitFor != context.targetAgent.decider.CurrentDriveType || levelReached) &&
                context.animParamIndex < context.animParamNames.Count - 1)
            {
                ++context.animParamIndex;
                agent.animator.SetTrigger(context.animParamNames[context.animParamIndex]);
            }
            */
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // If the other agent is not still focused on this DriveType exit - otherwise agent could be waiting forever
            // TODO: This whole system feels awkward - perhaps scrap it for the AgentEvent system
            //if (context.driveTypeToWaitFor != context.targetAgent.decider.CurrentDriveType)
            //    return true;

            bool levelReached = false;
            if (equalityType == EqualityType.GreaterThanOrEqual)
                levelReached = context.targetAgent.drives[context.driveTypeToWaitFor].GetLevel() >= driveLevelThreshold;
            else
                levelReached = context.targetAgent.drives[context.driveTypeToWaitFor].GetLevel() <= driveLevelThreshold;

            return levelReached;
            //        context.animParamIndex == context.animParamNames.Count - 1) &&
            //        agent.animator.GetCurrentAnimatorStateInfo(agent.agentType.idleLayer).IsName(agent.agentType.idleState);
        }

        public override void InterruptBehavior(Agent agent)
        {
            /*
            WaitForDriveBehaviorTypeContext context = (WaitForDriveBehaviorTypeContext)behaviorTypeContext;

            // Planner interrupted this Behavior in the middle of it
            // Exit middle state
            if (context.animParamIndex < context.animParamNames.Count - 1)
            {
                ++context.animParamIndex;
                agent.animator.SetTrigger(context.animParamNames[context.animParamIndex]);
            }
            */
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            // Add up wait times
            float time = 1f;
            //foreach (MinMaxCurve skillModifier in mapping.mappingType.waitTimes)
            //{
            //    time += skillModifier.Eval0to100(agent.CalculatedActionSkill(mapping.mappingType.actionType)) *
            //            agent.timeManager.RealTimeSecondsPerGameMinute();
            //}

            return time;
        }
    }
}