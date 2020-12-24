using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentType", menuName = "Total AI/Entity Types/Agent Type", order = 0)]
    public class AgentType : EntityType
    {
        public MovementType defaultMovementType;
        public AnimationType defaultAnimationType;
        public bool useAnimatorOverrides;
        public int idleLayer;
        public string idleState;
        public UtilityFunctionType defaultUtilityFunction;
        public MemoryType defaultMemoryType;
        public DeciderType defaultDeciderType;
        public HistoryType defaultHistoryType;
        public MappingType defaultNoPlansMappingType;
        public DriveType defaultNoPlansDriveType;

        [Header("Sensors")]
        public int maxNumDetectedEntities = 100;
        public List<SensorType> defaultSensorTypes;

        [Serializable]
        public class DefaultAction
        {
            public ActionType actionType;
            public float level;
            public float changeProbability;
            public float changeAmount;

            public static List<ActionType> ActionTypes(List<DefaultAction> defaultActions)
            {
                return defaultActions.ConvertAll(x => x.actionType);
            }
        }
        public List<DefaultAction> defaultActions;

        [Serializable]
        public class DefaultDrive
        {
            public DriveType driveType;
            public float startingLevel;
            public bool overrideDriveType;

            public float changePerGameHour;
            public AnimationCurve rateTimeCurve;
            public float maxTimeCurve;
            public float minTimeCurve;
        }
        public List<DefaultDrive> defaultDrives;

        [Serializable]
        public class AnimatorOverride
        {
            public AnimatorOverrideController controller;
            public List<int> conditionIndexes;
        }
        public List<AnimatorOverride> animatorOverrides;
        public List<InputCondition> animatorOverridesConditions;

        public override GameObject CreateEntity(int prefabVariantIndex, Vector3 position, Quaternion rotation, Vector3 scale, Entity creator)
        {
            GameObject prefab = prefabVariants[prefabVariantIndex];
            prefab.SetActive(false);
            GameObject newAgentGameObject = Instantiate(prefab, position, rotation);
            newAgentGameObject.transform.localScale = scale;
            newAgentGameObject.SetActive(true);
            prefab.SetActive(true);
            return newAgentGameObject;
        }

        // Creates an Agent of AgentType being in the entity's inventory
        public override Entity CreateEntityInInventory(Entity entity, int prefabVariantIndex, InventorySlot inventorySlot)
        {
            Debug.LogError("Trying to create an AgentType Entity inside an inventory.  Not supported yet.");
            return null;
        }

        // Returns the Mapping Types that this Agent Type can perform based on the Action Types and filtering by AgentType
        public HashSet<MappingType> AvailableMappingTypes()
        {
            HashSet<MappingType> mappingTypes = new HashSet<MappingType>();
            foreach (DefaultAction defaultAction in defaultActions)
            {
                mappingTypes.UnionWith(defaultAction.actionType.FilteredMappingTypes(this));
            }
            return mappingTypes;
        }

        public bool HasDriveType(DriveType driveType)
        {
            return defaultDrives.Exists(x => x.driveType == driveType);
        }

        public bool HasActionType(ActionType actionType)
        {
            return defaultActions.Exists(x => x.actionType == actionType);
        }
    }
}
