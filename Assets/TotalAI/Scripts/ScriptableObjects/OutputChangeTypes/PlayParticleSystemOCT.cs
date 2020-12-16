using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "PlayParticleSystemOCT", menuName = "Total AI/Output Change Types/Play ParticleSystem", order = 0)]
    public class PlayParticleSystemOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Play ParticleSystem</b>: Adds the selected ParticleSystem Prefab to the target.  Removes it when the ParticleSystem is done.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.UnityObject },
                unityObjectType = typeof(GameObject),
                unityObjectLabel = "VFX Prefab",
                usesFloatValue = true,
                floatLabel = "VFXSpot Index"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // TODO: Feels like checks like these should be done once at start of game - InitCheck?
            GameObject vfxPrefab = outputChange.unityObject as GameObject;
            if (vfxPrefab == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayVFXOCT: VFX Prefab is null.");
                return false;
            }

            GameObject vfxGameObject = Instantiate(vfxPrefab, target.GetVFXGameObject((int)outputChange.floatValue).transform);

            ParticleSystem particleSystem = vfxGameObject.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayVFXOCT: VFX Prefab has no particle system.");
                return false;
            }

            if (!particleSystem.isPlaying)
                particleSystem.Play();

            // TODO: Switch to pooling
            if (!particleSystem.main.loop)
                Destroy(vfxGameObject, particleSystem.main.duration);

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
