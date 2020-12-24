using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DriveUtilityUFT", menuName = "Total AI/Utility Function Types/Drive Utility", order = 0)]
    public class DriveUtilityUFT : UtilityFunctionType
    {
        // Current Utility Function - TODO: want to make it so SO allows you to define the formula
        // Utility = (DU + DU_INFLU * (1 - DU)) * Rate + SE_INFL * SUM(SE Utilities)

        //[Header("(DU + DU_INFLU * (1 - DU)) * Drive Reduction Rate + SE_INFL * SUM(SE Utilities)")]
        //[Space]
        [Header("Drive Utility Influence")]
        [Range(0, 1)]
        public float DU_INFLU; // Drive Utility Influence - 0-1 range
        [Header("Side Effects Influence")]
        [Range(0, 1)]
        public float SE_INFLU; // Side Effects Influence - 0-1 range

        public void OnEnable()
        {
            editorDescription = "This Utility Function takes into account the drive utility to moderate how " +
                                       "much impact the side effects have on the final utility.\n\n" +
                                       "<b>DU</b> = Drive Utility (0-1): How pressing is the need to reduce this drive?\n" +
                                       "<b>DU_INFLU</b> = DU Influence (0-1): \n" +
                                       "<b>SE</b> = Side Effects: Sum up all of the side effects from Mappings in the Plan.\n" +
                                       "<b>SE_INFLU</b> = SE Influence (0-1): \n\n" +
                                       "(DU + DU_INFLU * (1 - DU)) * Drive Reduction Rate +\nSE_INFL * SUM(SE Utilities)";
        }

        // driveRate should be positive
        public override float Evaluate(Agent agent, Mapping rootMapping, DriveType driveType,
                                       float driveAmount, float timeEst, float sideEffectsUtility)
        {

            float driveRate = -driveAmount / timeEst;
            float driveUtility = agent.drives[driveType].GetDriveUtility();

            if (verboseLogging)
            {
                Debug.Log("(" + driveUtility + " + " + DU_INFLU + " * (1 - " + driveUtility + ")) * " + driveRate + " + " + SE_INFLU + " * " + sideEffectsUtility);
                Debug.Log("DriveType Utility Multiplier = " + (driveUtility + DU_INFLU * (1 - driveUtility)));
            }
            return (driveUtility + DU_INFLU * (1 - driveUtility)) * driveRate + SE_INFLU * sideEffectsUtility;
        }

        public override string DisplayEquation(Agent agent, Mapping rootMapping, DriveType driveType,
                                               float driveAmount, float timeEst, float sideEffectsUtility)
        {
            float driveRate = -driveAmount / timeEst;
            float driveUtility = agent.drives[driveType].GetDriveUtility();

            return "(" + driveUtility + " DU + " + DU_INFLU + " DU_INFLU * (1 - " + driveUtility + " DU)) * " + driveRate + " Drive Rate + " +
                   SE_INFLU + " SE_INFLU * " + sideEffectsUtility + " SE";
        }

    }
}