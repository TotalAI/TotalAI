using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveType", menuName = "Total AI/Level Types/Drive Type", order = 0)]
    public class DriveType : LevelType
    {
        [Header("Should this Drive Type be shown in the upper left of the UI?")]
        public bool showInUI;

        [Header("Priority if tied - higher value has priority")]
        public int priority;

        [Header("Modify chance to continue on this Drive  - mulitiplied by Drive utility")]
        public float continueModifier = 1;

        [Header("Can cause Mappings to be interrupted")]
        public bool canCauseInterruptions;

        public enum SyncType { None, Attribute, Equation };
        [Header("Is the Drive level synced with an Attribute or with an Equation?")]
        public SyncType syncType;

        [Header("Attribute Info")]
        public AttributeType syncAttributeType;
        public enum SyncAttributeDirection { Same, Opposite };
        public SyncAttributeDirection syncAttributeDirection;

        [Header("Drive Type Equation")]
        public DriveTypeEquation driveTypeEquation;
        public bool includeInSECalculations;

        [Header("How important is the Drive based on Drive level?")]
        public AnimationCurve utilityCurve;

        public enum RateOfChangeType { Constant, TimeOfDayCurve };

        [Header("How does Drive Type's level change?")]
        public RateOfChangeType changeType;

        public float changePerGameHour;

        [Header("Rate based on time of day - 0-1 on x is 0-24 time of day so 0.5 is noon")]
        public AnimationCurve rateTimeCurve;

        [Header("These define range of y on curve - 0-1 on y is min-max")]
        public float minTimeCurve;
        public float maxTimeCurve;

        // Returns the Drive level based on the attributes level
        public float GetAttributeLevel(Agent agent)
        {
            if (!agent.attributes.ContainsKey(syncAttributeType))
                Debug.LogError(agent.name + ": Missing an Attribute Type (" + syncAttributeType + ") that is synced to a Drive Type (" + name + ")");

            float level;
            if (syncAttributeDirection == SyncAttributeDirection.Same)
                level = agent.attributes[syncAttributeType].GetNormalizedLevel() * 100f;
            else
                level = (1 - agent.attributes[syncAttributeType].GetNormalizedLevel()) * 100f;

            return level;
        }
        
        public string Abbreviations()
        {
            List<string> abbr = new List<string>();

            if (syncType == SyncType.Attribute)
                abbr.Add("AT");
            else if (syncType == SyncType.Equation)
                abbr.Add("EQ");

            if (changeType == RateOfChangeType.TimeOfDayCurve)
                abbr.Add("ToD");

            if (abbr.Count == 0)
                return "";
            return " (" + string.Join(",", abbr) + ")";
        }
    }
}