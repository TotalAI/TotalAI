using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "WaitBT", menuName = "Total AI/Behavior Types/Wait", order = 0)]
    public class WaitBT : BehaviorType
    {
        public class Context
        {
            public List<float> waitTimes;
            public int waitTimeIndex;
            public List<float> animSpeedMultipliers;
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
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            context.startTime = Time.time;
            context.waitTimeIndex = 0;
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            if (context.waitTimes[context.waitTimeIndex] > Time.time - context.startTime)
            {
                ++context.waitTimeIndex;
                context.startTime = Time.time;
            }
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Has the last waitTime finished?
            return context.waitTimeIndex == context.waitTimes.Count;
        }

        public override void InterruptBehavior(Agent agent)
        {
            // Don't need to do anything since the Agent is just waiting
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            float time = 0f;
            //for (int i = 0; i < mapping.mappingType.waitTimes.Count; i++)
            //{
            //    time += mapping.mappingType.waitTimes[i].Eval(agent.CalculatedActionSkill(mapping.mappingType.actionType));
            //}
            return time;
        }
    }
}
