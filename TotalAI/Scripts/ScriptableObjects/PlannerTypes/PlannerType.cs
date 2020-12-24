using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class PlannerType : ScriptableObject
    {
        public abstract void Setup(Agent agent);
        public abstract Plans CreatePlansForDriveType(Agent agent, DriveType driveType, bool checkingToInterrupt);
        public abstract void ReevaluateTargets(Agent agent, Mapping mapping);

        // This allows plannerTypes to save chosen Mapping when Decider Interrupts
        public abstract void NotifyOfInterrupt(Agent agent, Plans plans, Mapping rootMapping);
    }
}
