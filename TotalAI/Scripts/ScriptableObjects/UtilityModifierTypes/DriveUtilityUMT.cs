using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveUtilityUMT", menuName = "Total AI/Utility Modifier Types/Drive Utility", order = 0)]
    public class DriveUtilityUMT : UtilityModifierType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Drive Utility</b>: Returns the utility of the specified Drive Type.",
                usesLevelType = true,
                levelTypeLabel = "Drive Type",
                mostRestrictiveLevelType = typeof(DriveType)
            };
        }

        public override float Evaluate(UtilityModifier utilityModifer, Agent agent, Mapping mapping, out bool veto)
        {
            veto = false;
            return agent.drives[(DriveType)utilityModifer.levelType].GetDriveUtility();
        }
    }
}