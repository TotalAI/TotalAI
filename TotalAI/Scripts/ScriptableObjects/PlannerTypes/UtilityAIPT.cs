using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI.GOAP
{
    [CreateAssetMenu(fileName = "UtilityAIPT", menuName = "Total AI/Planner Types/Utility AI", order = 0)]
    public class UtilityAIPT : PlannerType
    {
        private Dictionary<Agent, Plans> nextPlans;

        private void OnEnable()
        {
            nextPlans = new Dictionary<Agent, Plans>();
        }

        public override void Setup(Agent agent)
        {
            nextPlans[agent] = null;
        }

        public override void NotifyOfInterrupt(Agent agent, Plans plans, Mapping rootMapping)
        {
            nextPlans[agent] = plans;
        }

        public override void ReevaluateTargets(Agent agent, Mapping mapping)
        {
            Debug.LogError("UtilityAIPT.ReevaluateTargets not implememted.  There is no chaining of Mappings so this is not needed.");
        }

        // Drive Type is ignored by Utility AI - It always returns all possible mappings
        public override Plans CreatePlansForDriveType(Agent agent, DriveType driveType, bool checkingToInterrupt)
        {
            if (!checkingToInterrupt && nextPlans[agent] != null)
            {
                // We may have already gotten the new plans from NotifyOfInterrupt (planned to see if interrupt is needed)
                // Use them instead of replanning
                Plans plans = nextPlans[agent];
                nextPlans[agent] = null;
                return plans;
            }
            
            return PossiblePlans(agent, driveType);
        }
        
        private Plans PossiblePlans(Agent agent, DriveType driveType)
        {
            List<Mapping> possibleMappings = new List<Mapping>();
            foreach (MappingType mappingType in agent.availableMappingTypes)
            {
                Mapping possibleMapping = new Mapping(mappingType);

                // If MappingType needs an Entity Target - find best target based on TargetFactors
                // TODO: Return all targets each one as a seperate Mapping - so Utility Modifiers can decide between them
                if (mappingType.ForEntityType())
                {
                    Entity target = BestEntityTarget(agent, possibleMapping);
                    if (target == null)
                        continue;

                    possibleMapping.target = target;
                }

                // Check non-entity target ICs
                bool passedAllChecks = true;
                foreach (InputCondition inputCondition in mappingType.NonEntityTypeInputConditions())
                {
                    if (!inputCondition.inputConditionType.Check(inputCondition, agent, possibleMapping, null, false))
                    {
                        passedAllChecks = false;
                        break;
                    }
                }
                if (!passedAllChecks)
                    continue;

                if (mappingType.HasAnyInventoryTargets())
                {
                    // TODO: Move this conditional inits into Mapping constructor
                    possibleMapping.inventoryTargets = new List<Entity>();
                    for (int i = 0; i < mappingType.inputConditions.Count; i++)
                    {
                        possibleMapping.inventoryTargets.Add(null);
                    }

                    passedAllChecks = true;
                    for (int i = 0; i < mappingType.inputConditions.Count; i++)
                    {
                        InputCondition inputCondition = mappingType.inputConditions[i];
                        if (inputCondition.RequiresInventoryTarget() && possibleMapping.inventoryTargets[i] == null)
                        {
                            // Finally make sure we can find all of the InventoryTargets
                            if (!SetInventoryTargets(agent, possibleMapping, inputCondition))
                            {
                                passedAllChecks = false;
                                break;
                            }
                        }
                    }
                    if (!passedAllChecks)
                        continue;
                }

                // Passed all checks - add it as a possible state to transition into
                possibleMapping.isComplete = true;
                possibleMappings.Add(possibleMapping);
            }

            return new Plans(driveType, possibleMappings);
        }

        // Return the best Target for this mapping - returns null if there is no Target
        // TODO: Does best - add in other selection algorithms
        private Entity BestEntityTarget(Agent agent, Mapping mapping)
        {
            List<MemoryType.EntityInfo> entityInfos;
            List<Entity> possibleTargets;

            List<InputCondition> entityTargetICs = mapping.mappingType.EntityTypeInputConditions();
            if (entityTargetICs == null)
                return null;

            List<EntityType> entityTypes = TypeGroup.PossibleEntityTypes(entityTargetICs, agent.totalAIManager.allEntityTypes);

            float searchRadius = mapping.mappingType.GetEntityTargetSearchRadius();
            if (searchRadius > 0)
                entityInfos = agent.memoryType.GetKnownEntities(agent, entityTypes, searchRadius, true);
            else
                entityInfos = agent.memoryType.GetKnownEntities(agent, entityTypes, -1, true);

            possibleTargets = CheckPossibleTargets(agent, mapping, entityTargetICs, entityInfos);
            
            List<KeyValuePair<Entity, float>> targetsRanked = SelectTarget(agent, mapping, possibleTargets, false);
            if (targetsRanked != null && targetsRanked.Count > 0)
            {
                return targetsRanked[0].Key;
            }

            return null;
        }

        private bool SetInventoryTargets(Agent agent, Mapping mapping, InputCondition inputCondition)
        {
            List<Entity> possibleTargets;
            List<KeyValuePair<Entity, float>> targetsRanked;

            List<TypeGroup> groupings = inputCondition.InventoryTypeGroups(mapping.mappingType, out List<int> indexesToSet);

            // Target is from the agent's inventory if the input condition does not require an Entity Target
            if (!inputCondition.RequiresEntityTarget())
            {
                List<EntityType> allAgentInventoryEntityTypes = agent.inventoryType.GetAllEntityTypes(agent);
                List<Entity> allAgentInventoryEntities = agent.inventoryType.GetAllEntities(agent, true);

                List<EntityType> entityTypes = TypeGroup.InAllTypeGroups(groupings, allAgentInventoryEntityTypes);
                possibleTargets = allAgentInventoryEntities.Where(x => entityTypes.Contains(x.entityType)).ToList();
            }
            else
            {
                List<EntityType> entityTypes = mapping.target.inventoryType.GetAllEntityTypes(mapping.target);
                entityTypes = TypeGroup.InAllTypeGroups(groupings, entityTypes);
                possibleTargets = mapping.target.inventoryType.GetAllEntities(mapping.target, entityTypes, false);
            }

            if (possibleTargets.Count == 0)
                return false;

            targetsRanked = SelectTarget(agent, mapping, possibleTargets, true);
            if (targetsRanked != null)
            {
                // TODO: Handle more selection algorithms - this is just best
                Entity target = targetsRanked[0].Key;
                foreach (int index in indexesToSet)
                {
                    mapping.inventoryTargets[index] = target;
                }
            }
            else
            {
                Debug.Log(agent.name + ": UtilityAIPT.BestInventoryTargets unable to find target - MT = " + mapping.mappingType);
                return false;
            }

            return true;
        }
        
        private List<Entity> CheckPossibleTargets(Agent agent, Mapping mapping, List<InputCondition> entityICs,
                                                 List<MemoryType.EntityInfo> entityInfos)
        {
            List<Entity> possibleEntities = new List<Entity>();
            foreach (MemoryType.EntityInfo entityInfo in entityInfos)
            {
                bool passedAllChecks = true;
                foreach (InputCondition inputCondition in entityICs)
                {
                    if (!inputCondition.inputConditionType.Check(inputCondition, agent, mapping, entityInfo.entity, false))
                    {
                        passedAllChecks = false;
                        break;
                    }
                }
                if (passedAllChecks)
                    possibleEntities.Add(entityInfo.entity);
            }

            return possibleEntities;
        }

        // TODO: Lots of performance optimization can be done on this - including caching results for other rootMappings?
        // Utility = weight0 * factor0 + weight1 * factor1 ... + weight2 * factor2
        // Any -1 factor is a veto - eliminates the entity from condsideration
        private List<KeyValuePair<Entity, float>> SelectTarget(Agent agent, Mapping mapping, List<Entity> entities, bool useInventoryTFs)
        {
            List<MappingType.TargetFactorInfo> factorInfos;
            Dictionary<Entity, float> utilities = new Dictionary<Entity, float>();
            foreach (Entity entity in entities)
            {
                float utility = 0f;
                if (useInventoryTFs && entity.inEntityInventory == agent)
                    factorInfos = mapping.mappingType.inventoryFactorInfos;
                else
                    factorInfos = mapping.mappingType.targetFactorInfos;

                if (factorInfos.Count == 0)
                {
                    Debug.LogError(agent.name + ": UtilityAIPT - MappingType (" + mapping.mappingType.name + ") has no " + (useInventoryTFs ? "Inventory" : "Target") +
                                   " Factors.  Please Fix.");
                    return null;
                }

                foreach (MappingType.TargetFactorInfo targetFactorInfo in factorInfos)
                {
                    // TODO: Should this pass in all of the EntityType ICs?
                    float factor = targetFactorInfo.targetFactor.Evaluate(agent, entity, mapping, useInventoryTFs);

                    if (factor == -1f)
                    {
                        utility = -1f;
                        break;
                    }
                    utility += targetFactorInfo.weight * factor;
                }

                //Debug.Log("SelectTarget: MappingType = " + name + " Entity = " + entity.name + " Utility = " + utility);

                if (utility != -1f)
                    utilities.Add(entity, utility);
            }

            if (utilities.Count == 0)
                return null;

            // Pick the target based on the selection criteria - Best, RandomWeighted, TopXPercentRandomWeighted
            // TODO: Implement other ones - This just does Best
            return utilities.OrderByDescending(x => x.Value).ToList();
        }

    }
}
