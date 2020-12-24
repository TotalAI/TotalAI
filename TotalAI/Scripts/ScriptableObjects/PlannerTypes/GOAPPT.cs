using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI.GOAP
{
    [CreateAssetMenu(fileName = "GOAPPPT", menuName = "Total AI/Planner Types/GOAP", order = 0)]
    public class GOAPPT : PlannerType
    {
        [Header("Max levels in the plan tree.")]
        public int maxLevels = 5;

        GOAPPlannerManager plannerManager;

        public override void Setup(Agent agent)
        {
            // TODO: Better way to find the GOAP Planner
            plannerManager = GameObject.Find("GOAPPlannerManager").GetComponent<GOAPPlannerManager>();

            if (plannerManager == null)
            {
                Debug.LogError("Setting up GOAPPlannerType and can't find GOAPPlannerManager in the scene.  Please add it to an empty object.");
            }

        }

        // Ignore this - will always replan
        // TODO: Optional use the plans to avoid replanning
        public override void NotifyOfInterrupt(Agent agent, Plans plans, Mapping rootMapping)
        {
        }

        public override Plans CreatePlansForDriveType(Agent agent, DriveType driveType, bool checkingToInterrupt)
        {
            return plannerManager.CreatePlansForDriveType(agent, agent.agentType, driveType);
        }

        public override void ReevaluateTargets(Agent agent, Mapping mapping)
        {
            plannerManager.ReevaluateTargets(agent, mapping);
        }

        // TODO: Lots of performance optimization can be done on this - including caching results for other rootMappings?
        // Utility = weight0 * factor0 + weight1 * factor1 ... + weight2 * factor2
        // Any -1 factor is a veto - eliminates the entity from condsideration
        public List<KeyValuePair<Entity, float>> SelectTarget(Agent agent, Mapping mapping, List<Entity> entities, bool useInventoryTFs)
        {
            List<MappingType.TargetFactorInfo> factorInfos;
            Dictionary<Entity, float> utilities = new Dictionary<Entity, float>();
            foreach (Entity entity in entities)
            {
                float utility = 0f;
                //if (useInventoryTFs && entity.inEntityInventory == agent)
                if (useInventoryTFs)
                {
                    factorInfos = mapping.mappingType.inventoryFactorInfos;
                }
                else
                {
                    factorInfos = mapping.mappingType.targetFactorInfos;
                }

                if (factorInfos.Count == 0)
                {
                    Debug.LogError(agent.name + ": MappingType (" + mapping.mappingType.name + ") has no " +
                                   (useInventoryTFs ? "Inventory" : "Target") + " Factors.  Please Fix.");
                    return null;
                }

                foreach (MappingType.TargetFactorInfo targetFactorInfo in factorInfos)
                {
                    // TODO: Should this pass in all of the EntityType ICs?
                    float factor = targetFactorInfo.targetFactor.Evaluate(agent, entity, mapping, useInventoryTFs);
                    //Debug.Log("SelectTarget: TargetFactor = " + targetFactorInfo.targetFactor.name + " Entity = " + entity.name + " Factor = " + factor);

                    if (factor == -1f)
                    {
                        utility = -1f;
                        break;
                    }
                    utility += targetFactorInfo.weight * factor;
                }

                //Debug.Log("SelectTarget: MappingType = " + mapping.mappingType.name + " Entity = " + entity.name + " Utility = " + utility);

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
