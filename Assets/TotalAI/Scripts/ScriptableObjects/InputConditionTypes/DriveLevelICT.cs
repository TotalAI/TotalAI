using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveLevelICT", menuName = "Total AI/Input Condition Types/Drive Level", order = 0)]
    public class DriveLevelICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Drive Level</b>: Is the Agent's Drive Level between min and max (inclusive)?",
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(DriveType),
                usesMinMax = true,
                minLabel = "Min Drive Level",
                maxLabel = "Max Drive Level"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            float driveLevel = agent.drives[(DriveType)inputCondition.levelType].GetLevel();
            if (driveLevel >= inputCondition.min && driveLevel <= inputCondition.max)
                return true;
            return false;
        }
    }
}
