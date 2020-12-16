using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "CreateEntityOCT", menuName = "Total AI/Output Change Types/Create Entity", order = 0)]
    public class CreateEntityOCT : OutputChangeType
    {
        // TODO: Get rid of this and split it into CreateAgent and CreateWorldObject
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Create Entity</b>: Creates a new Entity. Use Value to destroy after x seconds.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToSelf, OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.FloatValue, OutputChange.ValueType.Selector },
                usesEntityType = true,
                usesBoolValue = true,
                boolLabel = "Set Mapping Target To New Entity",
                //usesIntValue = true,
                //intLabel = "Prefab Variant Index",
                floatLabel = "Destroy After (s): 0 to Ignore"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            EntityType entityType = outputChange.entityType;

            // If this is an AgentEvent location could need to be synced with WorldObject
            // TODO: if self use inventory drop location?
            Vector3 position = target.transform.position;
            //int prefabVariantIndex = (int)outputChange.floatValue;
            int prefabVariantIndex = 0;
            if (entityType.prefabVariants == null || entityType.prefabVariants.Count <= prefabVariantIndex)
            {
                Debug.LogError(agent.name + ": CreateEntityOCT: " + entityType.name + " does not have enough Prefab Variants.  " +
                               "Tried to access Prefab Variant Index = " + prefabVariantIndex + " - Please Fix.");
                return false;
            }

            GameObject newGameObject = entityType.CreateEntity(prefabVariantIndex, position, entityType.prefabVariants[prefabVariantIndex].transform.rotation,
                                                               entityType.prefabVariants[prefabVariantIndex].transform.localScale, agent);
            Entity newEntity = newGameObject.GetComponent<Entity>();
            if (outputChange.boolValue)
                mapping.target = newEntity;

            if (outputChange.floatValue > 0f)
                newEntity.DestroySelf(null, outputChange.floatValue);

            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0f;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool PreMatch(OutputChange outputChange, MappingType outputChangeMappingType, InputCondition inputCondition,
                                      MappingType inputConditionMappingType, List<EntityType> allEntityTypes)
        {
            // The EntityType being created need match with the ICMT's EntityType TypeGroup Target
            List<TypeGroup> typeGroups = inputConditionMappingType.EntityTypeGroupings();

            if (outputChange.entityType.InAllTypeGroups(typeGroups))
                return true;
            return false;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            // All matching needed is done in PreMatch (since the specific EntityType being created needs to be specified)            
            return true;
        }
    }
}
