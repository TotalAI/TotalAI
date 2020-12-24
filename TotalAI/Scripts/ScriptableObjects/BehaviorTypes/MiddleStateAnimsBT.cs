using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MiddleStateAnimsBT", menuName = "Total AI/Behavior Types/Middle State Anims", order = 0)]
    public class MiddleStateAnimsBT : BehaviorType
    {
        public class Context
        {
            public List<string> animParamNames;
            public int animParamIndex;
            public float waitTime;
            public float startTime;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { },
                valueDescriptions = new string[] { }
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
            if (!agentsContexts.TryGetValue(agent, out Context context))
            {
                context = new Context();
                agentsContexts[agent] = context;
            }
            context.startTime = Time.time;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Start first animation
            if (context.animParamNames.Count > 0)
                agent.animationType.SetTrigger(agent, context.animParamNames[context.animParamIndex]);
        }


        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Wait until time to complete is done - then do animation
            if (Time.time - context.startTime > context.waitTime && context.animParamIndex < context.animParamNames.Count - 1)
            {
                ++context.animParamIndex;
                agent.animationType.SetTrigger(agent, context.animParamNames[context.animParamIndex]);
            }
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Has the last animation finished?  Handles case of having no animations just a wait time
            // TODO: Should there be a separate WaitBehavior?
            return ((context.animParamNames.Count == 0 && Time.time - context.startTime > context.waitTime) ||
                    context.animParamIndex == context.animParamNames.Count - 1) &&
                    agent.animationType.InIdleState(agent);
        }

        public override void InterruptBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Planner interrupted this Behavior in the middle of it
            // Exit middle state
            if (context.animParamIndex < context.animParamNames.Count - 1)
            {
                ++context.animParamIndex;
                agent.animationType.SetTrigger(agent, context.animParamNames[context.animParamIndex]);
            }
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            // Add up wait times
            float time = 0f;
            //foreach (MinMaxCurve skillModifier in mapping.mappingType.waitTimes)
            //{
            //    time += skillModifier.Eval0to100(agent.CalculatedActionSkill(mapping.mappingType.actionType)) *
            //            agent.timeManager.RealTimeSecondsPerGameMinute();
            //}

            return time;
        }
    }
}
