using System;
using UnityEngine;

namespace TotalAI {
    public class Attribute : Level
    {
        private Entity entity;
        private AttributeType attributeType;

        public Attribute(Entity entity, AttributeType attributeType, AttributeType.Data data)
        {
            //if (attributeType.usesMinMax && min > max)
            //    Debug.LogError(attributeType.name + ": Min > Max.  Please fix.");
            this.entity = entity;
            levelType = attributeType;
            this.attributeType = attributeType;
            attributeType.Initialize(entity, data);
        }

        public float GetNormalizedLevel()
        {
            if (!(attributeType is MinMaxFloatAT))
            {
                Debug.LogError("Called GetNormalizedLevel on Attribute (" + attributeType.name + ") that does not use Min Max.  Please Fix.");
            }

            return ((MinMaxFloatAT)attributeType).GetNormalizedLevel(entity);
        }

        public float ChangeLevelRelative(float amount)
        {
            return ChangeLevel(GetLevel() + amount);
        }

        private DriveType GetSyncedDriveType(Agent agent)
        {
            foreach (DriveType driveType in agent.drives.Keys)
            {
                if (driveType.syncType == DriveType.SyncType.Attribute && driveType.syncAttributeType == attributeType)
                {
                    return driveType;
                }
            }
            return null;
        }

        public override float ChangeLevel(float amount)
        {
            float initialValue = GetLevel();
            float newValue = amount;

            if (attributeType is OneFloatAT)
            {
                newValue = ((OneFloatAT.OneFloatData)attributeType.SetData(entity, new OneFloatAT.OneFloatData() { floatValue = amount })).floatValue;
            }
            else if (attributeType is MinMaxFloatAT)
            {
                MinMaxFloatAT.MinMaxFloatData data = new MinMaxFloatAT.MinMaxFloatData() { floatValue = amount };
                data = (MinMaxFloatAT.MinMaxFloatData)attributeType.SetData(entity, data);

                if (entity is Agent agent)
                {
                    DriveType syncedDriveType = GetSyncedDriveType(agent);
                    if (syncedDriveType != null)
                    {
                        agent.drives[syncedDriveType].ChangeAttributeSyncedDriveLevel(amount, GetMin(), GetMax());
                    }
                }
                newValue = data.floatValue;

                // TODO: Make 0.01f an option in Settings
                if (Mathf.Abs(newValue - initialValue) > 0.01f)
                    entity.RunEntityModifiers(null, EntityModifier.TriggerType.LevelChange);
            }
            else
            {
                return 0f;
            }

            return newValue;
        }

        public void ChangeLevelUsingDriveLevel(float driveLevel, DriveType.SyncAttributeDirection syncAttributeDirection)
        {
            if (attributeType is MinMaxFloatAT)
            {
                float normalizedValue;
                if (syncAttributeDirection == DriveType.SyncAttributeDirection.Same)
                    normalizedValue = driveLevel / 100f;
                else
                    normalizedValue = (100f - driveLevel) / 100f;

                ((MinMaxFloatAT)attributeType).SetValueFromNormalizedValue(entity, normalizedValue);
            }
        }

        public override float GetLevel()
        {
            float level = 0f;
            if (attributeType is OneFloatAT)
            {
                level = ((OneFloatAT.OneFloatData)attributeType.GetData(entity)).floatValue;
            }
            else if (attributeType is MinMaxFloatAT)
            {
                level = ((MinMaxFloatAT.MinMaxFloatData)attributeType.GetData(entity)).floatValue;
            }
            else
            {
                return 0;
            }

            // TODO: Performance - cache value with modifiers - only update it on an inventory Add or Remove
            if (entity.inventoryType.usesItemAttributeTypeModifiers)
            {
                level = entity.inventoryType.GetAlwaysItemAttributeTypeModifiers(entity, attributeType, level, ItemAttributeTypeModifier.ChangeType.Level);
            }

            return level;
        }

        public float GetMax()
        {
            float level = 0f;
            MinMaxFloatAT minMaxFloatAT = attributeType as MinMaxFloatAT;
            if (minMaxFloatAT != null)
            {
                level = minMaxFloatAT.GetMax(entity);
                if (entity.inventoryType.usesItemAttributeTypeModifiers)
                {
                    level = entity.inventoryType.GetAlwaysItemAttributeTypeModifiers(entity, attributeType, level, ItemAttributeTypeModifier.ChangeType.Max);
                }
            }
            return level;
        }

        public float GetMin()
        {
            float level = 0f;
            MinMaxFloatAT minMaxFloatAT = attributeType as MinMaxFloatAT;
            if (minMaxFloatAT != null)
            {
                level = minMaxFloatAT.GetMin(entity);
                if (entity.inventoryType.usesItemAttributeTypeModifiers)
                {
                    level = entity.inventoryType.GetAlwaysItemAttributeTypeModifiers(entity, attributeType, level, ItemAttributeTypeModifier.ChangeType.Min);
                }
            }
            return level;
        }

        public T GetEnumValue<T>()
        {
            EnumAT enumAT = attributeType as EnumAT;
            return enumAT.OptionValue<T>(entity);
        }
    }
}