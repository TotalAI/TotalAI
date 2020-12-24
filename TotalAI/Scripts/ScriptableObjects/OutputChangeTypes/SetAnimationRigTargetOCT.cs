using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "SetAnimationRigTargetOCT", menuName = "Total AI/Output Change Types/Set Animation Rig Target", order = 0)]
    public class SetAnimationRigTargetOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Set Animation Rig Target</b>: Warps the target of the animation rig to the Entity Target.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget, OutputChange.TargetType.ToInventoryTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None, OutputChange.ValueType.UnityObject },
                usesFloatValue = true,
                floatLabel = "Rig Layer Index",
                unityObjectType = typeof(Transform),
                unityObjectLabel = "Transform To Move Target To"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;
            if (!agent.animationType.ImplementsAnimationRigging())
            {
                Debug.Log(agent.name + ": SetAnimationRigTargetOCT - agent's AnimationType does not implement AnimationRigging.");
                return true;
            }
            
            Vector3 position;
            Quaternion rotation;
            if (outputChange.targetType == OutputChange.TargetType.ToInventoryTarget)
            {
                position = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].transform.position;
                position = agent.transform.InverseTransformPoint(position);
                rotation = Quaternion.identity;
            }
            else if (outputChange.targetType == OutputChange.TargetType.ToSelf)
            {
                Transform transform = (Transform)outputChange.unityObject;
                position = transform.position;
                rotation = transform.rotation;
            }
            else
            {
                position = agent.transform.InverseTransformPoint(target.transform.position);
                rotation = Quaternion.identity;
            }

            agent.animationType.WarpRigConstraintTargetTo(agent, (int)outputChange.floatValue, position, rotation);
            
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
