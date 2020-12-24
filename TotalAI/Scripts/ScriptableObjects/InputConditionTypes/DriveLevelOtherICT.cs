using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveLevelOtherICT", menuName = "Total AI/Input Condition Types/Drive Level Other", order = 0)]
    public class DriveLevelOtherICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Drive Level Other</b>: Is Drive Level for target Agent between min and max?",
                usesTypeGroupFromIndex = true,
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(DriveType),
                usesMinMax = true
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            Agent targetAgent = target as Agent;

            if (targetAgent != null)
            {
                DriveType driveType = (DriveType)inputCondition.levelType;
                if (targetAgent.ActiveDrives().ContainsKey(driveType))
                {
                    float driveLevel = targetAgent.drives[driveType].GetLevel();
                    if (driveLevel >= inputCondition.min && driveLevel <= inputCondition.max)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}