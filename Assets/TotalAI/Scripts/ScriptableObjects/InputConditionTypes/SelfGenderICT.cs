using System.Collections.Generic;
using UnityEngine;
namespace TotalAI
{
    [CreateAssetMenu(fileName = "SelfGenderICT", menuName = "Total AI/Input Condition Types/Self Gender", order = 0)]
    public class SelfGenderICT : InputConditionType
    {
        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Self Gender</b>: If the Agent a certain gender?",
                usesEnumValue = true,
                enumType = typeof(Agent.Gender)
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            return agent.gender == (Agent.Gender)inputCondition.enumValueIndex;
        }
    }
}