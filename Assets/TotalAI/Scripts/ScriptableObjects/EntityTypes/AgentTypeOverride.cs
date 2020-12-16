using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "AgentTypeOverride", menuName = "Total AI/Entity Types/Agent Type Override", order = 0)]
    public class AgentTypeOverride : EntityTypeOverride
    {
        [Header("Sensors")]
        public EntityOverrideType overrideSensorTypes;
        public List<SensorType> defaultSensorTypes;
        public int maxNumDetectedEntities = -1;

        [Header("Core Types")]
        public MappingType defaultNoPlansMappingType;
        public DriveType defaultNoPlansDriveType;
        public MemoryType defaultMemoryType;
        public DeciderType defaultDeciderType;
        public UtilityFunctionType defaultUtilityFunction;
        public HistoryType defaultHistoryType;
        public MovementType defaultMovementType;
        public AnimationType defaultAnimationType;
        public AnimatorOverrideController defaultAnimatorOverrideController;
        public int idleLayer;
        public string idleState;

        [Header("Actions")]
        public EntityOverrideType overrideActions;
        public List<ActionType> defaultActionTypes;  // Any actions this agent can perform
        public List<float> defaultActionLevels;  // ActionType skill levels will default to these values
        public List<float> defaultActionChangeProbabilities;  // ActionType skill levels change chance will default to these values
        public List<float> defaultActionChangeAmounts;  // ActionType skill levels change amount will default to these values

        // TODO: Add in ActionItemSkills

        [Header("Drives")]
        public EntityOverrideType overrideDrives;
        public List<DriveType> defaultDriveTypes;  // Any drives this agent starts with
        public List<float> defaultDriveLevels;  // DriveType levels will default to these values
        public List<float> defaultDriveChangesPerHour;  // Drives will change at this rate per hour (game hour)

        [Header("Rate based on time of day - 0-1 on x is 0-24 time of day so 0.5 is noon")]
        // The rate will be determined by this curve - 0-1 on x is 0-24 time of day - i.e. 0.5 is noon
        public List<AnimationCurve> defaultDriveChangesCurve;
        [Header("These define range of y on curve - 0-1 on y is min-max")]
        public List<float> defaultMaxDriveChangesCurve;
        public List<float> defaultMinDriveChangesCurve;
    }

}