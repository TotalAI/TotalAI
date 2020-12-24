using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TypeCategory", menuName = "Total AI/Type Category", order = 1)]
    public class TypeCategory : ScriptableObject
    {
        public TypeCategory parent;
        public List<TypeCategory> children;

        public bool IsCategoryOrAncestorOf(TypeCategory typeCategory)
        {
            if (typeCategory == this)
                return true;
            else if (parent == null)
                return false;

            return parent.IsCategoryOrAncestorOf(typeCategory);
        }

        public bool IsCategoryOrAncestorOf(List<TypeCategory> typeCategories)
        {
            foreach (TypeCategory typeCategory in typeCategories)
            {
                if (IsCategoryOrAncestorOf(typeCategory))
                    return true;
            }
            return false;
        }

        public bool IsCategoryOrDescendantOf(TypeCategory typeCategory)
        {
            if (typeCategory == this)
                return true;
            else if (children == null || children.Count == 0)
                return false;

            foreach (TypeCategory child in children)
            {
                if (child.IsCategoryOrDescendantOf(typeCategory))
                    return true;
            }
            return false;
        }

        public bool IsCategoryOrDescendantOf(List<TypeCategory> typeCategories)
        {
            foreach (TypeCategory typeCategory in typeCategories)
            {
                if (IsCategoryOrDescendantOf(typeCategory))
                    return true;
            }
            return false;
        }

        public bool IsCategoryOrDescendantOf(EntityType entityType)
        {
            return IsCategoryOrDescendantOf(entityType.typeCategories);
        }

        public List<InputOutputType> IsCategoryOrDescendantOf(List<InputOutputType> inputOutputTypes)
        {
            List<InputOutputType> result = IsCategory(inputOutputTypes);

            if (children != null)
            {
                foreach (TypeCategory child in children)
                {
                    result.AddRange(child.IsCategoryOrDescendantOf(inputOutputTypes));
                }
            }
            result.Sort(delegate (InputOutputType x, InputOutputType y) { return x.name.CompareTo(y.name); });
            return result.Distinct().ToList();
        }

        public List<InputOutputType> IsCategory(List<InputOutputType> inputOutputTypes)
        {
            return inputOutputTypes.FindAll(x => x.typeCategories != null && x.typeCategories.Contains(this));
        }

        public bool AnyInTypeCategory(List<InputOutputType> inputOutputTypes)
        {
            return inputOutputTypes.Exists(x => x.typeCategories.Contains(this));
        }

        public bool AnyInTypeCategory(List<Entity> entities)
        {
            List<EntityType> entityTypes = entities.Select(x => x.entityType).Distinct().ToList();
            return entityTypes.Exists(x => x.typeCategories.Contains(this));
        }

        public bool AnyInTypeCategoryOrDescendantOf(List<InputOutputType> inputOutputTypes)
        {
            List<TypeCategory> typeCategories = inputOutputTypes.SelectMany(x => x.typeCategories).Distinct().ToList();
            return IsCategoryOrDescendantOf(typeCategories);
        }

        public bool AnyInTypeCategoryOrDescendantOf(List<Entity> entities)
        {
            List<TypeCategory> typeCategories = entities.SelectMany(x => x.entityType.typeCategories).Distinct().ToList();
            return IsCategoryOrDescendantOf(typeCategories);
        }

#if UNITY_EDITOR
        public static void UpdateTypeCategoriesChildren()
        {
            List<TypeCategory> allTypeCategories = new List<TypeCategory>();
            var guids = AssetDatabase.FindAssets("t:TypeCategory");
            foreach (string guid in guids)
            {
                allTypeCategories.Add((TypeCategory)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
            }

            foreach (TypeCategory typeCategory in allTypeCategories)
            {
                typeCategory.children = new List<TypeCategory>();
            }
            foreach (TypeCategory typeCategory in allTypeCategories)
            {
                EditorUtility.SetDirty(typeCategory);
                foreach (TypeCategory typeCategory2 in allTypeCategories)
                {
                    if (typeCategory.parent == typeCategory2)
                    {
                        typeCategory2.children.Add(typeCategory);
                        break;
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
