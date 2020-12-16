using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "PlaySoundFromATMOCT", menuName = "Total AI/Output Change Types/Play Sound From ATM", order = 0)]
    public class PlaySoundFromATMOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Play Sound From ATM</b>: Plays an Audio Clip from inventory target's matching Action Type Modifier.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget},
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
                usesInventoryTypeGroupMatchIndex = true,
                usesFloatValue = true,
                floatLabel = "SFXSpot Index",
                usesBoolValue = true,
                boolLabel = "Don't Play if Playing",
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            OutputChange previousOutputChange = outputChange.Previous(mapping.mappingType);
            HistoryType.OutputChangeLog outputChangeLog = agent.historyType.GetOutputChangeLog(agent, mapping, previousOutputChange);

            if (outputChangeLog == null)
            {
                Debug.LogError(agent.name + ": PlaySoundFromATMOCT - unable to find OC status in HistoryType OutputChangeLog.");
                return false;
            }

            int tagEnumIndex;
            if (outputChangeLog.succeeded)
                tagEnumIndex = 1;
            else
                tagEnumIndex = 2;

            // This uses mapping.target since target will be the agent if TargetType is ToSelf
            ActionType otherActionType = null;
            if (mapping.target is Agent targetAgent && targetAgent.decider.CurrentMapping != null)
                otherActionType = targetAgent.decider.CurrentMapping.mappingType.actionType;

            Entity inventoryTarget = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];
            List<AudioClip> audioClips = agent.inventoryType.GetItemSoundEffects(agent, mapping.mappingType.actionType, otherActionType,
                                                                                 ItemCondition.ModifierType.Used, false, tagEnumIndex);
            if (audioClips == null || audioClips.Count == 0)
            {
                Debug.Log(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlaySoundFromATMOCT: No Audio Clips found.");
                return false;
            }

            AudioClip audioClip = audioClips[Random.Range(0, audioClips.Count)];
            
            if (audioClip == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlaySoundFromATMOCT: Audio Clip is null.");
                return false;
            }

            AudioSource audioSource = target.GetSFXGameObject((int)outputChange.floatValue).GetComponent<AudioSource>();
            if (audioSource == null)
            {
                if (mapping != null)
                    Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlaySoundFromATMOCT: target " +
                                   target.name + " has no AudioSource Component.");
                else
                    Debug.LogError(agent.name + ": PlaySoundFromATMOCT: target " + target.name + " has no AudioSource Component.");
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
