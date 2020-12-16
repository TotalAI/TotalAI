using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "PlaySoundOCT", menuName = "Total AI/Output Change Types/Play Sound", order = 0)]
    public class PlaySoundOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Play Sound</b>: Plays the selected Audio Clip.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget},
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.UnityObject, OutputChange.ValueType.Selector },
                unityObjectType = typeof(AudioClip),
                unityObjectLabel = "Audio Clip",
                usesFloatValue = true,
                floatLabel = "SFXSpot Index",
                usesBoolValue = true,
                boolLabel = "Don't Play if Playing"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            AudioClip audioClip = null;
            if (outputChange.valueType == OutputChange.ValueType.UnityObject)
            {
                audioClip = outputChange.unityObject as AudioClip;
            }
            else
            {
                audioClip = outputChange.selector.GetEnumValue<AudioClip>(agent, mapping);
            }

            // TODO: Feels like checks like these should be done once at start of game - InitCheck?
            if (audioClip == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlaySoundOCT: Audio Clip is null.");
                return false;
            }

            AudioSource audioSource = target.GetSFXGameObject((int)outputChange.floatValue).GetComponent<AudioSource>();
            if (audioSource == null)
            {
                if (mapping != null)
                    Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlaySoundOCT: target " +
                                   target.name + " has no AudioSource Component.");
                else
                    Debug.LogError("PlaySoundOCT: target " + target.name + " has no AudioSource Component.");
                return false;
            }

            if (!audioSource.isPlaying || !outputChange.boolValue)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
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
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
