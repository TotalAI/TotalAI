using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "HasTagICT", menuName = "Total AI/Input Condition Types/Has Tag", order = 0)]
    public class HasTagICT : InputConditionType
    {
        public enum Scope { HasTagIsOwner, HasTagButNotOwner, HasTagIgnoreOwnership, DoesNotHaveOwnerTag, DoesNotHaveTag }

        private new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Has Tag</b>: Various TagType filters for the Target EntityType.",
                usesTypeGroup = true,
                usesLevelType = true,
                mostRestrictiveLevelType = typeof(TagType),
                usesEnumValue = true,
                enumType = typeof(Scope)
            };
        }

        public override bool Check(InputCondition inputCondition, Agent agent, Mapping mapping, Entity target, bool isRecheck)
        {
            if (target == null)
                return false;

            TagType tagType = (TagType)inputCondition.levelType;
            List<Tag> tagList;
            Scope scope = (Scope)inputCondition.enumValueIndex;
            bool success = false;
            switch (scope)
            {
                case Scope.HasTagIsOwner:
                    success = target.tags.TryGetValue(tagType, out tagList) && tagList.Any(x => x.relatedEntity == agent);
                    break;
                case Scope.HasTagButNotOwner:
                    success = target.tags.TryGetValue(tagType, out tagList) && !tagList.Any(x => x.relatedEntity == agent);
                    break;
                case Scope.HasTagIgnoreOwnership:
                    success = target.tags.ContainsKey(tagType);
                    break;
                case Scope.DoesNotHaveOwnerTag:
                    success = !target.tags.TryGetValue(tagType, out tagList) || !tagList.Any(x => x.relatedEntity == agent);
                    break;
                case Scope.DoesNotHaveTag:
                    success = !target.tags.ContainsKey(tagType);
                    break;
            }
            return success;
        }
    }
}