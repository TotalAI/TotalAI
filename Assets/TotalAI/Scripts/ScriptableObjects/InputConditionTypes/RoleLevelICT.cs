using System;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "RoleLevelICT", menuName = "Total AI/Input Condition Types/Role Level", order = 0)]
    public class RoleLevelICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Role Level</b>: Does the agent have this role and enough level at it?",
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(RoleType),
                usesMinMax = true,
                minLabel = "Min Role Level",
                maxLabel = "Max Role Level",
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (agent.roles.TryGetValue((RoleType)inputCondition.levelType, out Role role))
            {
                if (role.GetLevel() >= inputCondition.min && role.GetLevel() <= inputCondition.max)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
