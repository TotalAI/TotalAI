using System;
using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "CurrentActionTypeICT", menuName = "Total AI/Input Condition Types/Current Action Type", order = 0)]
    public class CurrentActionTypeICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Current Action Type</b>: Is the Agent currently performing or just finished any of the specified Action Types?",
                usesLevelTypes = true,
                mostRestrictiveLevelType = typeof(ActionType),
                usesBoolValue = true,
                boolLabel = "Include GoTo ActionTypes?"
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (isRecheck)
                return true;

            Mapping currentMapping = agent.decider.CurrentMapping;
            if (currentMapping == null)
            {
                currentMapping = agent.decider.PreviousMapping;
                if (currentMapping == null)
                    return false;
            }

            if (!inputCondition.boolValue && currentMapping.parent != null && currentMapping.mappingType.goToActionType != null &&
                currentMapping.mappingType.goToActionType == currentMapping.parent.mappingType.actionType)
                currentMapping = currentMapping.parent;

            if (inputCondition.levelTypes.Contains(currentMapping.mappingType.actionType))
                return true;

            return false;
        }
    }
}
