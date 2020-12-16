using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AnimationBT", menuName = "Total AI/Behavior Types/Animation", order = 0)]
    public class AnimationBT : BehaviorType
    {
        public class Context
        {
            public Animation animation;
            public List<Selector> attributeSelectors;
            public Entity target;
        }

        private Dictionary<Agent, Context> agentsContexts;

        private new void OnEnable()
        {
            agentsContexts = new Dictionary<Agent, Context>();

            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] { typeof(EnumStringAT), typeof(MinMaxFloatAT) },
                valueDescriptions = new string[] { "Animation Trigger Param Name", "Animation Speed Mulitplier" }
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
            context.target = behaviorContext.target;
        }

        public override void StartBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            string triggerName = context.attributeSelectors[0].GetEnumValue<string>(agent, agent.decider.CurrentMapping);
            float speedMultiplier = context.attributeSelectors[1].GetFloatValue(agent, agent.decider.CurrentMapping);

            agent.animationType.SetAnimationSpeed(agent, speedMultiplier);
            agent.animationType.SetTrigger(agent, triggerName);
        }

        public override void UpdateBehavior(Agent agent)
        {
            Context context = agentsContexts[agent];

            if (context.target != null)
            {
                Agent targetAgent = context.target as Agent;
                if (targetAgent != null && !targetAgent.isAlive)
                {
                    InterruptBehavior(agent);
                }
            }

            // Start next Animation if the previous one is finished
            // TODO: Figure out where idleLayer and idleState should go
            /*
            if (agent.animator.GetCurrentAnimatorStateInfo(agent.agentType.idleLayer).IsName(agent.agentType.idleState))
            {
                ++context.animParamIndex;
                // Set animation speed multiplier if it is set
                if (context.animSpeedMultipliers != null && context.animSpeedMultipliers.Count > context.animParamIndex)
                {
                    agent.animator.SetFloat("SpeedMultiplier", context.animSpeedMultipliers[context.animParamIndex]);
                }
                agent.animator.SetTrigger(context.animParamNames[context.animParamIndex]);
            }
            */
        }

        public override bool IsFinished(Agent agent)
        {
            Context context = agentsContexts[agent];

            // Has the last animation finished?
            bool finished = agent.animationType.InIdleState(agent);

            if (finished)
            {
                agent.animationType.ResetAnimationSpeed(agent);
            }

            return finished;
        }

        public override void InterruptBehavior(Agent agent)
        {

            // Go back to idle immediately
            agent.animationType.GoToIdleState(agent);

            /*
            AnimationsBehaviorTypeContext context = (AnimationsBehaviorTypeContext)behaviorTypeContext;

            // Planner interrupted this Behavior in the middle of it
            if (!agent.animator.GetCurrentAnimatorStateInfo(0).IsName(agent.agentType.idleState))
            {
                // Agent is not in groundState - try to get it into it
                ++context.animParamIndex;
                agent.animator.SetTrigger(context.animParamNames[context.animParamIndex]);
            }
            */
        }
        
        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            /*
            // Add up the time for all animations to play
            List<string> animParamNames = mapping.AnimatorParams(agent);
            RuntimeAnimatorController runtimeAnimatorController = agent.animator.runtimeAnimatorController;

            float time = 0f;
            for (int i = 0; i < animParamNames.Count; i++)
            {
                for (int j = 0; j < runtimeAnimatorController.animationClips.Length; j++)
                {
                    // This requires the Toggle is the same name as the clip
                    // TODO: Fix this - maybe add clip directly into the Mapping?
                    if (runtimeAnimatorController.animationClips[j].name == animParamNames[i])
                    {
                        if (i < mapping.mappingType.animationSpeedMultipliers.Count)
                        {
                            time += runtimeAnimatorController.animationClips[j].length *
                                mapping.mappingType.animationSpeedMultipliers[i].Eval(agent.CalculatedActionSkill(mapping.mappingType.actionType));
                        }
                        else
                        {
                            time += runtimeAnimatorController.animationClips[j].length;
                        }
                        
                    }
                }
            }

            return time;
            */
            return 1f;
        }
    }
}
