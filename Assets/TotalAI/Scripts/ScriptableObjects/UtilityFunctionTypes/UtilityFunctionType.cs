using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class UtilityFunctionType : ScriptableObject
    {
        protected readonly bool verboseLogging = true;

        [HideInInspector]
        public string editorDescription;

        public abstract float Evaluate(Agent agent, Mapping rootMapping, DriveType driveType,
                                       float driveAmount, float timeEst, float sideEffectsUtility);
        public abstract string DisplayEquation(Agent agent, Mapping rootMapping, DriveType driveType,
                                       float driveAmount, float timeEst, float sideEffectsUtility);
    }
}