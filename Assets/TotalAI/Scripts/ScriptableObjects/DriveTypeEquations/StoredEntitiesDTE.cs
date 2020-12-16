using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "StoredEntitiesDTE", menuName = "Total AI/Drive Type Equations/Stored Entities", order = 0)]
    public class StoredEntitiesDTE : DriveTypeEquation
    {
        public List<OutputChangeType> inventoryIncreasesOTCs;
        public List<OutputChangeType> inventoryDecreasesOTCs;

        [Header("Type Category of the Entity Types that are in inventory")]
        public TypeCategory entityStoredCategory;
        [Header("Type Category of the Entity Types that are holding the inventory")]
        public TypeCategory storageCategory;
        [Header("Drive Level will be 0 if amount is at or higher than Constant Max")]
        public float constantMax;

        [Header("Storage must be tagged with this Tag to be included - Leave None to ignore")]
        public TagType storageTagType;
        [Header("The Entity of the above Tag Type must be in same Faction as this Agent")]
        public bool entityTaggedSameFaction;

        public override float GetEquationMax(Agent agent)
        {
            return constantMax;
        }

        public override float GetEquationRawLevel(Agent agent)
        {
            if (storageTagType != null && entityTaggedSameFaction)
            {
                if (agent.faction == null)
                {
                    Debug.LogError(agent.name + ": StoredEntitiesDTE - entityTaggedSameFaction is checked but Agent is not in a Faction.");
                    return 0f;
                }
                List<Entity> factionMembers = agent.faction.GetAllAgents().Cast<Entity>().ToList();
                return agent.totalAIManager.AmountOfStoredEntities(storageCategory, entityStoredCategory, storageTagType, factionMembers);
            }
            return agent.totalAIManager.AmountOfStoredEntities(storageCategory, entityStoredCategory, storageTagType);
        }

        public override float ChangeInOutputChange(Agent agent, DriveType driveType, OutputChange outputChange, Mapping mapping)
        {
            if (storageTagType != null)
            {
                if (!mapping.target.HasTag(storageTagType))
                    return 0f;

                if (entityTaggedSameFaction)
                {
                    if (agent.faction == null)
                    {
                        Debug.LogError(agent.name + ": StoredEntitiesDTE - entityTaggedSameFaction is checked but Agent is not in a Faction.");
                        return 0f;
                    }
                    List<Entity> factionMembers = agent.faction.GetAllAgents().Cast<Entity>().ToList();
                    if (!factionMembers.Any(x => mapping.target.tags[storageTagType].Select(y => y.relatedEntity).Contains(x)))
                        return 0f;
                }
            }

            // Want to find any inventory changes for storageCategory EntityTypes - where they gained or lost inventoryCategory EntityTypes
            // TODO: Handle parent Categories?
            if (inventoryIncreasesOTCs.Contains(outputChange.outputChangeType))
            {
                // The Target EntityType has to have the storageCategory
                // The outputChange.inputOutputType has to have the inventoryCategory
                if (mapping.target != null && mapping.target.entityType.typeCategories.Contains(storageCategory) &&
                    mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType.typeCategories.Contains(entityStoredCategory))
                {
                    return outputChange.outputChangeType.CalculateAmount(agent, mapping.target, outputChange, mapping);
                }
            }
            else if (inventoryDecreasesOTCs.Contains(outputChange.outputChangeType))
            {
                // The Target EntityType has to have the storageCategory
                // The outputChange.inputOutputType has to have the inventoryCategory
                if (mapping.target != null && mapping.target.entityType.typeCategories.Contains(storageCategory) &&
                    mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex].entityType.typeCategories.Contains(entityStoredCategory))
                {
                    return -outputChange.outputChangeType.CalculateAmount(agent, mapping.target, outputChange, mapping);
                }
            }

            return 0;
        }
    }
}
