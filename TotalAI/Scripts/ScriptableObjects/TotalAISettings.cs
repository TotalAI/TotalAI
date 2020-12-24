using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TotalAISettings", menuName = "Total AI/Settings", order = 1)]
    public class TotalAISettings : ScriptableObject
    {
        [Tooltip("Rounds the drive utility basically creating buckets.  For example rounding to 1 " +
                 "there will be 11 buckets (0.0, 0.1, ..., 0.9, 1.0) and rounding to 2 would have 101 buckets.")]
        [Range(1, 4)]
        public int roundDriveUtility = 2;
        public bool for2D;
        public string scriptableObjectsDirectory;
        public string prefabsDirectory;
        public InputConditionType driveLevelICT;
        public OutputChangeType driveLevelOCT;

        public InventoryType defaultInventoryType;
        public List<MovementType> movementTypes;
    }
}
