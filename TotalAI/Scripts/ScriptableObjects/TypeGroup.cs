using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TypeGroup", menuName = "Total AI/Type Group", order = 1)]
    public class TypeGroup : ScriptableObject
    {
        public enum EntityTypeEnum { EntityType, AgentType, WorldObjectType, AgentEventType }
        //public enum TypeCategoryJoinType { All, Any, None }

        [SerializeField]
        private EntityTypeEnum entityTypeEnum = 0;
        //[SerializeField]
        //private TypeCategoryJoinType typeCategoryJoinType;
        [SerializeField]
        private List<TypeCategory> typeCategories = null;
        [SerializeField]
        [Header("Use these lists to include or exclude specific EntityTypes")]
        private List<EntityType> includeEntityTypes = null;
        private List<EntityType> excludeEntityTypes = null;

        // Caches every EntityType match for this Grouping for performance
        private List<EntityType> entityTypeMatches;
        private TotalAIManager manager;

        private void OnEnable()
        {
            manager = FindObjectOfType<TotalAIManager>();
            if (manager != null)
                ResetCachedMatches();
        }

        public void ResetCachedMatches()
        {
            entityTypeMatches = new List<EntityType>();
            
            if (manager != null && manager.allEntityTypes != null)
            {
                foreach (EntityType entityType in manager.allEntityTypes)
                {
                    if (InGroup(entityType, false))
                        entityTypeMatches.Add(entityType);
                }
            }
        }

        static public TypeGroup CreateTypeGroup(EntityType entityType)
        {
            TypeGroup typeGroup = CreateInstance<TypeGroup>();
            typeGroup.includeEntityTypes = new List<EntityType>() { entityType };
            typeGroup.ResetCachedMatches();
            return typeGroup;
        }

        static public TypeGroup CreateTypeGroup(TypeCategory typeCategory)
        {
            TypeGroup typeGroup = CreateInstance<TypeGroup>();
            typeGroup.typeCategories = new List<TypeCategory>() { typeCategory };
            typeGroup.ResetCachedMatches();
            return typeGroup;
        }

        public bool AnyMatches(List<EntityType> entityTypes)
        {
            if (entityTypeMatches != null)
                return entityTypes.Any(x => entityTypeMatches.Contains(x));
            return entityTypes.Any(x => InGroup(x));
        }

        public bool AnyMatches(List<Entity> entities)
        {
            List<EntityType> entityTypes = entities.Select(x => x.entityType).Distinct().ToList();
            if (entityTypeMatches != null)
                return entityTypes.Any(x => entityTypeMatches.Contains(x));
            return entityTypes.Any(x => InGroup(x));
        }

        public List<EntityType> GetMatches(List<EntityType> entityTypes)
        {
            if (entityTypeMatches != null)
                return entityTypeMatches.Intersect(entityTypes).ToList();

            List<EntityType> matches = new List<EntityType>();

            foreach (EntityType entityType in entityTypes)
            {
                if (InGroup(entityType))
                    matches.Add(entityType);
            }

            return matches;
        }

        // Does the passed in EntityType fit this grouping?
        public bool InGroup(EntityType entityType, bool useCache = true)
        {
            // If cache exists use it
            if (useCache && entityTypeMatches != null)
                return entityTypeMatches.Contains(entityType);

            // If a specific include/exclude EntityTypes are selected check them first
            if (includeEntityTypes != null && includeEntityTypes.Count > 0)
            {
                if (includeEntityTypes.Contains(entityType))
                    return true;
            }
            if (excludeEntityTypes != null && excludeEntityTypes.Count > 0)
            {
                if (excludeEntityTypes.Contains(entityType))
                    return false;
            }

            // If there are included EntityTypes and no TypeCategories then only allow ones in the include
            if (includeEntityTypes != null && includeEntityTypes.Count > 0 &&
                (typeCategories == null || typeCategories.Count == 0))
                return false;

            // TypeCategory match and reject any ones that fail match
            if (!CategoriesMatch(entityType.typeCategories))
                return false;

            // Finaly do general Type level checks
            switch (entityTypeEnum)
            {
                case EntityTypeEnum.EntityType:
                    if (entityType is EntityType) return true;
                    break;
                case EntityTypeEnum.AgentType:
                    if (entityType is AgentType) return true;
                    break;               
                case EntityTypeEnum.WorldObjectType:
                    if (entityType is WorldObjectType) return true;
                    break;
                case EntityTypeEnum.AgentEventType:
                    if (entityType is AgentEventType) return true;
                    break;
            }

            return false;
        }

        private bool CategoriesMatch(List<TypeCategory> targetTypeCategories)
        {
            if (typeCategories == null || typeCategories.Count == 0)
                return true;

            // TODO: For now its an in ANY (at least one) - add in all and none later
            foreach (TypeCategory typeCategory in typeCategories)
            {
                if (typeCategory != null && typeCategory.IsCategoryOrDescendantOf(targetTypeCategories))
                    return true;
            }
            return false;            
        }

        // Given a list of ICs returns all EntityTypes that fit all of the ICs Groupings
        public static List<EntityType> PossibleEntityTypes(List<InputCondition> inputConditions, List<EntityType> entityTypes,
                                                           bool forInventoryTypeGroups = false)
        {
            if (inputConditions == null || inputConditions.Count == 0)
                return null;

            List<TypeGroup> typeGroups;
            if (forInventoryTypeGroups)
                typeGroups = inputConditions.Select(x => x.GetInventoryTypeGroup()).Distinct().ToList();
            else
                typeGroups = inputConditions.Select(x => x.GetTypeGroup()).Distinct().ToList();

            return InAllTypeGroups(typeGroups, entityTypes, false);
        }

        // Filters entityTypes by the list of TypeGroups - if a TypeGroup is null it ignores it
        public static List<EntityType> InAllTypeGroups(List<TypeGroup> typeGroups, List<EntityType> entityTypes, bool onlyFirst = false)
        {
            if (typeGroups == null || typeGroups.Count == 0)
                return null;

            List<EntityType> matches = new List<EntityType>();

            foreach (EntityType entityType in entityTypes)
            {
                bool inAllGroups = true;
                foreach (TypeGroup grouping in typeGroups)
                {
                    if (grouping != null && !grouping.InGroup(entityType))
                    {
                        inAllGroups = false;
                        break;
                    }
                }
                if (inAllGroups)
                    matches.Add(entityType);
            }
            return matches;
        }

        // Uses this Grouping to filter in place the list of entityTypes
        public void FilterInPlace(List<EntityType> entityTypes)
        {
            int numTypes = entityTypes.Count();
            for (int i = numTypes - 1; i >= 0; i--)
            {
                EntityType entityType = entityTypes[i];
                if (!InGroup(entityType))
                    entityTypes.Remove(entityType);
            }
        }

        public override string ToString()
        {
            string asString = name + " (" + entityTypeEnum + ") " + (typeCategories == null ? "No TypeCategories" : typeCategories.Count.ToString());

            if (includeEntityTypes != null)
                asString += ":" + includeEntityTypes.Count;

            return asString;
        }
    }
}
