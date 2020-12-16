
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [System.Serializable]
    public class InputCondition
    {
        public enum MatchType { TypeGroup, TypeCategory, EntityType }

        public InputConditionType inputConditionType;

        [SerializeField]
        private MatchType matchType = 0;
        [SerializeField]
        private TypeGroup typeGroupMatch = null;
        [SerializeField]
        private TypeCategory typeCategoryMatch = null;
        [SerializeField]
        private EntityType entityTypeMatch = null;
        private TypeGroup generatedTypeGroup;

        [SerializeField]
        private MatchType inventoryMatchType = 0;
        [SerializeField]
        private TypeGroup inventoryTypeGroupMatch = null;
        [SerializeField]
        private TypeCategory inventoryTypeCategoryMatch = null;
        [SerializeField]
        private EntityType inventoryEntityTypeMatch = null;
        public int sharesInventoryTypeGroupWith;
        private TypeGroup generatedInventoryTypeGroup;

        public LevelType levelType;
        public List<LevelType> levelTypes;
        public EntityType entityType;
        public List<EntityType> entityTypes;
        public float floatValue;
        public int intValue;
        public bool boolValue;
        public string stringValue;
        public int enumValueIndex;
        public float min;
        public float max;
        public MinMaxCurve actionSkillCurve;

        public InputCondition()
        {
        }

        public InputCondition(DriveType driveType, InputConditionType inputConditionType, bool boolValue)
        {
            levelType = driveType;
            this.inputConditionType = inputConditionType;
            this.boolValue = boolValue;
        }

        // Returns the TypeGroup to use for this IC - will generate one
        // if there isn't one already made for TypeCategory or EntityType MatchType
        // forceNewTypeGroup is used by Editor to make sure the Group is up to date
        public TypeGroup GetTypeGroup(bool forceNewTypeGroup = false)
        {
            if (matchType == MatchType.TypeGroup)
                return typeGroupMatch;

            if (generatedTypeGroup != null && !forceNewTypeGroup)
                return generatedTypeGroup;

            GenerateTypeGroup();
            return generatedTypeGroup;
        }

        public void GenerateTypeGroup()
        {
            if (matchType == MatchType.TypeCategory)
                generatedTypeGroup = TypeGroup.CreateTypeGroup(typeCategoryMatch);
            else
                generatedTypeGroup = TypeGroup.CreateTypeGroup(entityTypeMatch);
        }

        // Returns the InventoryTypeGroup to use for this IC - will generate one
        // if there isn't one already made for TypeCategory or EntityType MatchType
        // forceNewInventoryTypeGroup is used by Editor to make sure the Group is up to date
        public TypeGroup GetInventoryTypeGroup(bool forceNewInventoryTypeGroup = false)
        {
            if (inventoryMatchType == MatchType.TypeGroup)
                return inventoryTypeGroupMatch;

            if (generatedInventoryTypeGroup != null && !forceNewInventoryTypeGroup)
                return generatedInventoryTypeGroup;

            GenerateInventoryTypeGroup();
            return generatedInventoryTypeGroup;
        }

        public void GenerateInventoryTypeGroup()
        {
            if (inventoryMatchType == MatchType.TypeCategory)
                generatedInventoryTypeGroup = TypeGroup.CreateTypeGroup(inventoryTypeCategoryMatch);
            else
                generatedInventoryTypeGroup = TypeGroup.CreateTypeGroup(inventoryEntityTypeMatch);
        }

        public bool RequiresEntityTarget()
        {
            return inputConditionType == null ? false : inputConditionType.typeInfo.usesTypeGroup ||
                                                        inputConditionType.typeInfo.usesTypeGroupFromIndex;
        }

        public bool RequiresInventoryTarget()
        {
            return inputConditionType == null ? false : inputConditionType.typeInfo.usesInventoryTypeGroup ||
                                                        inputConditionType.typeInfo.usesInventoryTypeGroupShareWith;
        }

        public bool AnyInventoryGroupingMatches(MappingType mappingType, List<EntityType> entityTypes)
        {
            List<TypeGroup> groupings = InventoryTypeGroups(mappingType, out List<int> indexesToSet);
            List<EntityType> matches = TypeGroup.InAllTypeGroups(groupings, entityTypes, true);
            return matches != null && matches.Count > 0;
        }

        // Returns all Groupings that this InventoryGrouping needs to match based on sharesInventoryGroupWith
        // TODO: Have a way to ignore the indexesToSet - don't always need it
        public List<TypeGroup> InventoryTypeGroups(MappingType mappingType, out List<int> indexesToSet)
        {
            indexesToSet = new List<int>();
            if (!inputConditionType.typeInfo.usesInventoryTypeGroup)
                return null;

            List<TypeGroup> groupings = new List<TypeGroup>() { GetInventoryTypeGroup() };

            // See if this shares InventoryGroupings with any other IC
            // See if inputCondition shares its InventoryGrouping with any other ICs
            int thisIndex = -1;
            int[] sharesWith = new int[mappingType.inputConditions.Count];

            for (int i = 0; i < mappingType.inputConditions.Count; i++)
            {

                InputCondition inputCondition = mappingType.inputConditions[i];
                if (inputCondition.inputConditionType.typeInfo.usesInventoryTypeGroupShareWith)
                    sharesWith[i] = inputCondition.sharesInventoryTypeGroupWith;
                else
                    sharesWith[i] = -1;

                if (inputCondition == this)
                {
                    thisIndex = i;
                    indexesToSet.Add(i);
                }
            }

            // Need to know if this sharesWith or if any other ICs share with this one
            for (int i = 0; i < mappingType.inputConditions.Count; i++)
            {
                if (sharesWith[i] == thisIndex)
                {
                    groupings.Add(mappingType.inputConditions[i].GetInventoryTypeGroup());
                    indexesToSet.Add(i);
                }
                else if (i == thisIndex && sharesWith[i] != -1)
                {
                    groupings.Add(mappingType.inputConditions[sharesWith[i]].GetInventoryTypeGroup());
                    indexesToSet.Add(sharesWith[i]);
                }
            }

            if (groupings.Any(x => x == null))
            {
                Debug.LogError("MappingType " + mappingType.name + " Has null InventoryGroupings in ICs that need them.  Please fix.");
                return null;
            }

            return groupings;
        }

        public override string ToString()
        {
            string mappingAsString = "";

            if (inputConditionType != null)
                mappingAsString = inputConditionType.name;

            if (levelType != null)
                mappingAsString += " LT =" + levelType.name;
            if (entityType != null)
                mappingAsString += " ET =" + entityType.name;

            if (inputConditionType != null)
            {
                if (inputConditionType.typeInfo.usesBoolValue)
                    mappingAsString += " bool = " + boolValue;
                if (inputConditionType.typeInfo.usesFloatValue)
                    mappingAsString += " float = " + floatValue;
                if (inputConditionType.typeInfo.usesStringValue)
                    mappingAsString += " string = " + stringValue;
                if (inputConditionType.typeInfo.usesMinMax)
                    mappingAsString += " min/max = " + min + "-" + max;
                if (inputConditionType.typeInfo.usesActionSkillCurve)
                    mappingAsString += " ASC = " + actionSkillCurve;
                if (inputConditionType.typeInfo.usesEnumValue)
                    mappingAsString += " enum = " + inputConditionType.typeInfo.enumType.GetEnumNames()[enumValueIndex];
            }
            return mappingAsString;
        }

    }
}
