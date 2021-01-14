using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class Drive : Level
    {
        private AnimationCurve utilityCurve;
        private float changePerGameHour;
        private AnimationCurve rateTimeCurve;
        private float maxTimeCurve;
        private float minTimeCurve;

        private DriveType driveType;
        private Agent agent;

        public Drive(Agent agent, DriveType driveType, float level, AnimationCurve utilityCurve, float changePerGameHour,
                     AnimationCurve rateTimeCurve = null, float minTimeCurve = 0f, float maxTimeCurve = 0f)
        {
            this.agent = agent;
            levelType = driveType;
            this.driveType = driveType;
            this.level = level;
            this.utilityCurve = utilityCurve;
            this.changePerGameHour = changePerGameHour;
            this.rateTimeCurve = rateTimeCurve;
            this.minTimeCurve = minTimeCurve;
            this.maxTimeCurve = maxTimeCurve;
        }

        public override float GetLevel()
        {
            float driveLevel;
            if (driveType.syncType == DriveType.SyncType.None)
            {
                driveLevel = level;
            }
            else if (driveType.syncType == DriveType.SyncType.Equation)
            {
                if (driveType.driveTypeEquation == null || !Application.isPlaying)
                    return 0f;
                driveLevel = driveType.driveTypeEquation.GetEquationLevel(agent);
            }
            else
            {
                driveLevel = driveType.GetAttributeLevel(agent);
            }
            return driveLevel;
        }

        public float GetDriveUtility()
        {
            // TODO: Accuracy should be a setting - TotalAISettings?
            float driveUtility = (float)System.Math.Round(utilityCurve.Evaluate(GetLevel() / 100f), agent.totalAIManager.settings.roundDriveUtility);

            if (agent.decider.PreviousDriveType == driveType && driveType.continueModifier != 1f)
            {
                Debug.Log("ContinueModifier applied for " + driveType.name + " value = " + driveType.continueModifier);
                driveUtility *= driveType.continueModifier;
            }

            Debug.Log(driveType.name + ": Drive Utility = " + driveUtility);

            return driveUtility;
        }

        public override float ChangeLevel(float amount)
        {
            float actualAmount = amount;

            // If the drive uses an equation we don't want to change it directly
            if (driveType.syncType == DriveType.SyncType.Equation)
                return 0f;
            else if (driveType.syncType == DriveType.SyncType.Attribute)
                level = driveType.GetAttributeLevel(agent);

            level += amount;

            //if (driveType.forFaction)
            //    agent.faction.UpdateOtherMembersDriveLevel(agent, driveType, amount);

            // Don't let needs go below zero or above maxLevel
            // TODO: Switch above to just use Mathf.clamp
            if (level < 0f)
            {
                actualAmount = level - amount;
                level = 0f;
            }
            else if (level > 100f)
            {
                actualAmount = amount - (level - 100f);
                level = 100f;
            }

            // Update Attribute if its attached to this Drive Type
            if (driveType.syncType == DriveType.SyncType.Attribute)
            {
                agent.attributes[driveType.syncAttributeType].ChangeLevelUsingDriveLevel(level, driveType.syncAttributeDirection);
            }

            // Check agent's AgentModifiers to see if change triggered any of them
            if (actualAmount != 0f)
                agent.RunEntityTriggers(null, EntityTrigger.TriggerType.LevelChange);
            
            return actualAmount;
        }

        public void ChangeAttributeSyncedDriveLevel(float attributeLevel, float min, float max)
        {
            // Need to normalize attributeLevel to be in the 0-100 range
            if (driveType.syncAttributeDirection == DriveType.SyncAttributeDirection.Same)
                level = (attributeLevel - min) / (max - min) * 100f;
            else
                level = (1f - (attributeLevel - min) / (max - min)) * 100f;
        }

        public void ChangeFactionDriveLevel(float amount)
        {
            // The amount has already been checked by original change update to original faction member
            level += amount;
            agent.RunEntityTriggers(null, EntityTrigger.TriggerType.LevelChange);
        }
        
        public float CurrentDriveChangeRate(float minutesIntoDay)
        {
            if (driveType.changeType == DriveType.RateOfChangeType.TimeOfDayCurve)
            {
                if (Application.isPlaying)
                {
                    float rawChange = rateTimeCurve.Evaluate(minutesIntoDay / (24f * 60f));
                    float range = maxTimeCurve - minTimeCurve;
                    return rawChange * range + minTimeCurve;
                }
                return 0f;
            }

            // Return the agent's constant change rate
            return changePerGameHour;
        }

    }

}