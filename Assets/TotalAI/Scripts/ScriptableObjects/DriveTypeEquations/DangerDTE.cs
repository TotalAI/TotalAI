using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "DangerDTE", menuName = "Total AI/Drive Type Equations/Danger", order = 0)]
    public class DangerDTE : DriveTypeEquation
    {
        // The Danger Level depends on the number of hostile Agents that were just detected
        // For each hostile use the distance to hostile to scale danger amount

        // TODO: Can this be passed in and specified for each Agent - Add Max to Drive Refactor
        [Header("Maximum Danger Points")]
        public float constantMax;

        [Header("Caps Distance - Any distance over this will be set to max")]
        public float maxDistance;

        [Header("Normalized distance is input - returns 0-1 value")]
        public AnimationCurve distanceImpact;

        [Header("Caps RLevel - Anything over this will have no danger")]
        public float maxRLevel;

        [Header("Normalized RLevel is input - returns 0-1 value")]
        public AnimationCurve rLevelImpact;

        public override float GetEquationMax(Agent agent)
        {
            return constantMax;
        }

        public override float GetEquationRawLevel(Agent agent)
        {
            float dangerLevel = 0;

            foreach (MemoryType.EntityInfo entityInfo in agent.memoryType.GetShortTermMemory(agent))
            {
                float rLevel = entityInfo.rLevel;
                if (rLevel >= maxRLevel)
                    continue;

                float distance = Mathf.Min(entityInfo.distance, maxDistance);

                dangerLevel += rLevelImpact.Evaluate(rLevel / maxRLevel) * distanceImpact.Evaluate(distance / maxDistance);
            }

            return dangerLevel;
        }

        public override float ChangeInOutputChange(Agent agent, DriveType driveType, OutputChange outputChange, Mapping mapping)
        {
            // No way to get accurate reduction in Danger Level estimate - just go with 1 for now
            return 1f;
        }

        public override float GetEquationLevel(Agent agent)
        {
            float max = GetEquationMax(agent);
            float rawLevel = GetEquationRawLevel(agent);
            return (rawLevel > max ? max : rawLevel) / max * 100;
        }

        public override float CalculateEquationDriveLevelChange(Agent agent, DriveType driveType, Mapping mapping, OutputChange outputChange)
        {
            // TODO: Hard to know - need to reexamine this when there are multiple ways to reduce danger
            return -5f;
        }
    }
}
