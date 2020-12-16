using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ChangeMaterialOCT", menuName = "Total AI/Output Change Types/Change Material", order = 0)]
    public class ChangeMaterialOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Change Material</b>: Changes material of target.  Can be set to change back after X seconds.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.UnityObject, OutputChange.ValueType.Selector },
                unityObjectType = typeof(Material),
                unityObjectLabel = "New Material",
                usesFloatValue = true,
                floatLabel = "Change Back in Seconds : 0 to Ignore"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // TODO: Feels like checks like these should be done once at start of game - InitCheck?
            if (!(outputChange.unityObject is Material material))
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - ChangeMaterialOCT: Material is invalid.");
                return false;
            }

            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - ChangeMaterialOCT: target (" +
                               target.name + ") has no renderer.");
                return false;
            }

            Material oldMaterial = renderer.material;
            renderer.material = material;

            if (outputChange.floatValue > 0f)
            {
                agent.StartCoroutine(RevertMaterial(renderer, oldMaterial, outputChange.floatValue));
            }

            return true;
        }

        private IEnumerator RevertMaterial(Renderer renderer, Material oldMaterial, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            renderer.material = oldMaterial;
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
