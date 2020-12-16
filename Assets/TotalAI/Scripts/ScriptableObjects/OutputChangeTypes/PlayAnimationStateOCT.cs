using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "PlayAnimationStateOCT", menuName = "Total AI/Output Change Types/Play Animation State", order = 0)]
    public class PlayAnimationStateOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Play Animation State</b>: Directly plays a State in the Animator. " +
                              "If Selector is set OutputChange will use it and not the string value.  Selector can return a string or int (hash).",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.StringValue, OutputChange.ValueType.Selector },
                stringLabel = "String as 'Layer.StateName'",
                selectorLabel = "Selector Name or StateNameHash",
                usesBoolValue = true,
                boolLabel = "Wait Until Animation Is Finished",
                usesFloatValue = true,
                floatLabel = "Animator Layer Index"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // World Objects don't have AnimationTypes - they have to have an animator on them to work
            Animator animator = null;
            if (target is WorldObject)
            {
                animator = target.GetComponent<Animator>();
                // TODO: Feels like checks like these should be done once at start of game - InitCheck?
                if (animator == null)
                {
                    Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayAnimationStateOCT: target " +
                                   target.name + " has no animator.  WorldObject must have an animator on them to use this OCT.");
                    return false;
                }
            }

            string paramName = null;
            int paramNameHash = -1;
            if (outputChange.valueType == OutputChange.ValueType.Selector)
            {
                Type selectorForType = outputChange.selector.SelectorForType();
                if (selectorForType == null)
                {
                    Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayAnimationStateOCT: " +
                                   "Selector is not valid.  Please fix.");
                    return false;
                }

                if (selectorForType == typeof(string))
                {
                    paramName = outputChange.selector.GetEnumValue<string>(agent, mapping);
                }
                else if (selectorForType == typeof(int))
                {
                    paramNameHash = outputChange.selector.GetEnumValue<int>(agent, mapping);
                }
                else
                {
                    Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayAnimationStateOCT: " +
                                   "Selector is not for a string or an int.  Please fix.");
                    return false;
                }
            }
            else
            {
                paramName = outputChange.stringValue;
            }

            int layerIndex = (int)outputChange.floatValue;
            if (animator != null)
            {
                if (paramName != null)
                    animator.Play(paramName, layerIndex);
                else
                    animator.Play(paramNameHash, layerIndex);

                if (outputChange.boolValue)
                {
                    float waitTime = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip.length;
                    agent.behavior.SetIsWaiting(waitTime);
                }
            }
            else
            {
                Agent targetAgent = target as Agent;
                if (paramName != null)
                    targetAgent.animationType.PlayAnimationState(targetAgent, paramName, layerIndex);
                else
                    targetAgent.animationType.PlayAnimationState(targetAgent, paramNameHash, layerIndex);

                if (outputChange.boolValue)
                {
                    float waitTime = targetAgent.animationType.GetCurrentClipDuration(agent, layerIndex);
                    agent.behavior.SetIsWaiting(waitTime);
                }
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            // Destroying items so return the negative of the value of the items        
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
