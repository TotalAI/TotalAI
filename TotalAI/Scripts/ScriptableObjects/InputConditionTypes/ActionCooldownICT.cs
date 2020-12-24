using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "ActionCooldownICT", menuName = "Total AI/Input Condition Types/Action Cooldown", order = 0)]
    public class ActionCooldownICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Action Cooldown</b>: How long after an Action Type before Agent can do the same Action Type again?",
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(ActionType),
                usesFloatValue = true,
                floatLabel = "Time In Game Minutes"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            float lastTime = agent.historyType.GetLastTimePerformedActionType(agent, (ActionType)inputCondition.levelType);

            if (lastTime == -1f || Time.time - lastTime >= inputCondition.floatValue * agent.timeManager.RealTimeSecondsPerGameMinute())
                return true;

            return false;
        }
    }
}
