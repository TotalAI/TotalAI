using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AnimatorAT", menuName = "Total AI/Animation Types/Animator", order = 0)]
    public class AnimatorAT : AnimationType
    {
        public string animationSpeedName = "AnimationSpeed";

        private Dictionary<Agent, Animator> animators;
        
        public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
        {
            public AnimationClipOverrides(int capacity) : base(capacity) { }

            public AnimationClip this[string name]
            {
                get { return Find(x => x.Key.name.Equals(name)).Value; }
                set
                {
                    int index = FindIndex(x => x.Key.name.Equals(name));
                    if (index != -1)
                        this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
                }
            }
        }

        private Dictionary<Agent, AnimatorOverrideController> overrideControllers;
        private Dictionary<Agent, List<bool>> animatorsOverrideIsActives;
        private Dictionary<Agent, AnimationClipOverrides> clipOverrides;

        protected void OnEnable()
        {
            animators = new Dictionary<Agent, Animator>();
            overrideControllers = new Dictionary<Agent, AnimatorOverrideController>();
            animatorsOverrideIsActives = new Dictionary<Agent, List<bool>>();
            clipOverrides = new Dictionary<Agent, AnimationClipOverrides>();
        }

        public override void SetupAgent(Agent agent, bool forEditor = false)
        {
            if (agent.agentType.idleState == "")
            {
                Debug.LogError(agent.name + ": AgentType " + agent.agentType + " has a blank idle state.  Please fix.");
                return;
            }
            Animator animator = agent.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError(agent.name + ": Using AnimatorAT but missing an Animator component.  Please add.");
                return;
            }

            // TODO: Should be a setting on the SO
            animator.keepAnimatorControllerStateOnDisable = true;
            animators[agent] = animator;

            if (agent.agentType.useAnimatorOverrides && !forEditor)
            {
                animatorsOverrideIsActives[agent] = new List<bool>();
                foreach (AgentType.AnimatorOverride animatorOverride in agent.agentType.animatorOverrides)
                {
                    animatorsOverrideIsActives[agent].Add(false);
                }

                AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                foreach (AgentTypeOverride agentTypeOverride in agent.entityTypeOverrides)
                {
                    if (agentTypeOverride.defaultAnimatorOverrideController != null)
                        animatorOverrideController = agentTypeOverride.defaultAnimatorOverrideController;
                }
                animator.runtimeAnimatorController = animatorOverrideController;
                overrideControllers[agent] = animatorOverrideController;

                AnimationClipOverrides overrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
                animatorOverrideController.GetOverrides(overrides);
                clipOverrides[agent] = overrides;
            }

        }

        public override void UpdateAnimations(Agent agent)
        {
            List<AgentType.AnimatorOverride> activeOverrides = new List<AgentType.AnimatorOverride>();
            bool needChange = false;
            for (int i = 0; i < agent.agentType.animatorOverrides.Count; i++)
            {
                AgentType.AnimatorOverride animatorOverride = agent.agentType.animatorOverrides[i];
                bool isActive = animatorsOverrideIsActives[agent][i];
                bool passes = true;
                foreach (int index in animatorOverride.conditionIndexes)
                {
                    InputCondition condition = agent.agentType.animatorOverridesConditions[index];
                    if (!condition.inputConditionType.Check(condition, agent, null, null, false))
                    {
                        passes = false;
                        break;
                    }
                }

                if (passes)
                    activeOverrides.Add(animatorOverride);

                if (isActive && !passes)
                {
                    animatorsOverrideIsActives[agent][i] = false;
                    needChange = true;
                }
                else if (!isActive && passes)
                {
                    animatorsOverrideIsActives[agent][i] = true;
                    needChange = true;
                }
            }

            if (needChange)
            {
                // Priority is based on location in List (AgentType.animatorOverrides) - ones at the top (lower index) have higher priority
                // So go from bottom of active list to top
                if (activeOverrides.Count == 0)
                {
                    ClearOverrides(agent);
                }
                else
                {
                    for (int i = activeOverrides.Count - 1; i >= 0; i--)
                    {
                        ApplyOverride(agent, activeOverrides[i].controller);
                    }
                }
            }
        }

        private void ApplyOverride(Agent agent, AnimatorOverrideController overrideController)
        {
            AnimationClipOverrides currentOverrides = clipOverrides[agent];

            AnimationClipOverrides newOverrides = new AnimationClipOverrides(overrideController.overridesCount);
            overrideController.GetOverrides(newOverrides);

            foreach (KeyValuePair<AnimationClip, AnimationClip> item in newOverrides)
            {
                if (item.Value != null)
                    currentOverrides[item.Key.name] = newOverrides[item.Key.name];
            }
            overrideControllers[agent].ApplyOverrides(currentOverrides);
        }

        private void ClearOverrides(Agent agent)
        {
            AnimationClipOverrides currentOverrides = clipOverrides[agent];
            for (int i = 0; i < currentOverrides.Count; i++)
            {
                currentOverrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(currentOverrides[i].Key, null);
            }
            overrideControllers[agent].ApplyOverrides(currentOverrides);
        }

        public override Vector3 DeltaPosition(Agent agent)
        {
            return animators[agent].deltaPosition;
        }

        public override Vector3 RootPosition(Agent agent)
        {
            return animators[agent].rootPosition;
        }

        public override float GetCurrentClipDuration(Agent agent, int layerIndex)
        {
            // Get the first clip of the current state in layerIndex
            if (animators[agent].GetCurrentAnimatorClipInfoCount(layerIndex) > 0)
            {
                AnimatorClipInfo animatorClipInfo = animators[agent].GetCurrentAnimatorClipInfo(layerIndex)[0];

                return animatorClipInfo.clip.length;
            }

            return -1f;
        }

        public override string[] GetTriggerParamNames(Agent agent, bool forEditor)
        {
            if (forEditor)
            {
                UnityEditor.Animations.AnimatorController ac = animators[agent].runtimeAnimatorController as UnityEditor.Animations.AnimatorController;

                return ac.parameters.ToList().FindAll(x => x.type == AnimatorControllerParameterType.Trigger)
                                                       .Select(x => x.name).ToArray();
            }

            return animators[agent].parameters.ToList().FindAll(x => x.type == AnimatorControllerParameterType.Trigger)
                                                       .Select(x => x.name).ToArray();
        }

        public override void SetFloat(Agent agent, string name, float value)
        {
            animators[agent].SetFloat(name, value);
        }

        public override void SetFloat(Agent agent, string name, float value, float dampTime, float deltaTime)
        {
            animators[agent].SetFloat(name, value, dampTime, deltaTime);
        }

        public override void SetBool(Agent agent, string name, bool value)
        {
            animators[agent].SetBool(name, value);
        }

        public override void SetTrigger(Agent agent, string name)
        {
            animators[agent].SetTrigger(name);
        }

        public override void SetFloat(Agent agent, int id, float value)
        {
            animators[agent].SetFloat(id, value);
        }

        public override void SetFloat(Agent agent, int id, float value, float dampTime, float deltaTime)
        {
            animators[agent].SetFloat(id, value, dampTime, deltaTime);
        }

        public override void SetBool(Agent agent, int id, bool value)
        {
            animators[agent].SetBool(id, value);
        }

        public override void SetTrigger(Agent agent, int id)
        {
            animators[agent].SetTrigger(id);
        }

        public override void PlayAnimationState(Agent agent, string stateName, int layerIndex = -1)
        {
            animators[agent].Play(stateName, layerIndex);
        }

        public override void PlayAnimationState(Agent agent, int stateNameHash, int layerIndex = -1)
        {
            animators[agent].Play(stateNameHash, layerIndex);
        }

        public override bool InIdleState(Agent agent)
        {
            // Make sure its not transitioning out of idle
            //AnimatorStateInfo nextStateInfo = animators[agent].GetNextAnimatorStateInfo(agent.agentType.idleLayer);
            //Debug.Log("InIdleState: nextStateInfo is Idle = " + nextStateInfo.IsName("Idle"));
            
            return animators[agent].GetCurrentAnimatorStateInfo(agent.agentType.idleLayer).IsName(agent.agentType.idleState) &&
                   !animators[agent].IsInTransition(0);
        }

        public override void GoToIdleState(Agent agent)
        {
            animators[agent].Play(agent.agentType.idleState, agent.agentType.idleLayer);
        }

        public override void SetAnimationSpeed(Agent agent, float speed)
        {
            animators[agent].SetFloat(animationSpeedName, speed);
        }

        public override void ResetAnimationSpeed(Agent agent)
        {
            animators[agent].SetFloat(animationSpeedName, 1f);
        }

        public override void Enable(Agent agent)
        {
            animators[agent].enabled = true;
        }

        public override void Disable(Agent agent)
        {
            animators[agent].enabled = false;
        }

    }
}
