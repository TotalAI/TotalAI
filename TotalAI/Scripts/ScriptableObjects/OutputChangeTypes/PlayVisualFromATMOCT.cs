using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "PlayVisualFromATMOCT", menuName = "Total AI/Output Change Types/Play Visual From ATM", order = 0)]
    public class PlayVisualFromATMOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Play Visual From ATM</b>: Plays a Visual Effect from inventory target's matching Item Visual Effects.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget,
                    OutputChange.TargetType.ToInventoryTarget},
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
                usesInventoryTypeGroupMatchIndex = true,
                usesFloatValue = true,
                floatLabel = "VFXSpot Index",
                usesBoolValue = true,
                boolLabel = "Don't Add if Playing"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // This uses mapping.target since target will be the agent if TargetType is ToSelf
            ActionType otherActionType = null;
            if (mapping.target is Agent targetAgent && targetAgent.decider.CurrentMapping != null)
                otherActionType = targetAgent.decider.CurrentMapping.mappingType.actionType;

            Entity inventoryTarget = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];
            List<GameObject> gameObjects = agent.inventoryType.GetItemVisualEffects(agent, mapping.mappingType.actionType, otherActionType,
                                                                                    ItemCondition.ModifierType.Used, false);
            if (gameObjects == null || gameObjects.Count == 0)
                return false;

            GameObject vfxPrefab = gameObjects[Random.Range(0, gameObjects.Count)];
            
            if (vfxPrefab == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayVisualFromATMOCT: No Visual Effects found.");
                return false;
            }

            GameObject vfxGameObject = Instantiate(vfxPrefab, target.GetVFXGameObject((int)outputChange.floatValue).transform);

            ParticleSystem particleSystem = vfxGameObject.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError(agent.name + ": MappingType: " + mapping.mappingType.name + " - PlayVisualFromATMOCT: VFX Prefab has no particle system.");
                return false;
            }

            if (!particleSystem.isPlaying)
                particleSystem.Play();

            // TODO: Switch to pooling
            // TODO: Add in time to destroy for looping ones
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
