using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "UtilityAIUFT", menuName = "Total AI/Utility Function Types/Utility AI", order = 0)]
    public class UtilityAIUFT : UtilityFunctionType
    {

        public void OnEnable()
        {
            editorDescription = "This Utility Function only considers the rootMapping's Utility Modifiers";
        }

        public override float Evaluate(Agent agent, Mapping rootMapping, DriveType driveType,
                                       float driveAmount, float timeEst, float sideEffectsUtility)
        {
            if (rootMapping.mappingType.utilityModifierInfos == null || rootMapping.mappingType.utilityModifierInfos.Count == 0)
            {
                Debug.LogError(agent.name + ": UtilityAIUFT - Mapping Type " + rootMapping.mappingType.name + " has no Utility Modifiers.");
                return 0f;
            }
            float totalUtility = 0f;
            float totalWeight = 0f;
            foreach (MappingType.UtilityModifierInfo utilityModifierInfo in rootMapping.mappingType.utilityModifierInfos)
            {
                UtilityModifier utilityModifier = utilityModifierInfo.utilityModifier;
                float weight = utilityModifierInfo.weight;
                float utility = utilityModifier.utilityModifierType.Evaluate(utilityModifier, agent, rootMapping, out bool veto);

                if (verboseLogging)
                {
                    Debug.Log(agent.name + ": UtilityAIUFT - utilityModifier = " + utilityModifier.name + ": utility = " + utility + " - weight = " + weight);
                }

                if (veto)
                    return 0f;
                totalUtility += utility * weight;
                totalWeight += weight;
            }

            if (verboseLogging)
            {
                Debug.Log(agent.name + ": UtilityAIUFT - totalUtility = " + totalUtility + " - totalWeight = " + totalWeight);
            }

            return totalUtility / totalWeight;
        }

        public override string DisplayEquation(Agent agent, Mapping rootMapping, DriveType driveType,
                                               float driveAmount, float timeEst, float sideEffectsUtility)
        {
            return "Weighted Average of Utility Modifiers";
        }

    }
}