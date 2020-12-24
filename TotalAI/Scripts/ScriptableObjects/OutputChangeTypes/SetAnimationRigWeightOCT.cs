using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "SetAnimationRigWeightOCT", menuName = "Total AI/Output Change Types/Set Animation Rig Weight", order = 0)]
    public class SetAnimationRigWeightOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Set Animation Rig Weight</b>: Gradually changes the weight of the animation rig to the Selector float value.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.Selector },
                usesFloatValue = true,
                floatLabel = "Rig Layer Index"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            if (!agent.animationType.ImplementsAnimationRigging())
            {
                Debug.Log(agent.name + ": SetAnimationRigWeightOCT - agent's AnimationType does not implement AnimationRigging.  Skipping OCT.");
                return true;
            }

            float weight = outputChange.selector.GetFloatValue(agent, mapping);
            agent.animationType.SetRigWeight(agent, (int)outputChange.floatValue, weight);
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
