using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "LevelOtherTF", menuName = "Total AI/Target Factors/Level Other", order = 0)]
    public class LevelOtherTF : TargetFactor
    {
        [Header("Pick any LevelType except TagType - Use a reverse curve for choosing the lowest level.")]
        public LevelType levelType;

        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            if (levelType == null)
            {
                Debug.LogError(name + ": LevelType is not set.");
                return 0f;
            }

            Agent otherAgent = entity as Agent;
            if (otherAgent == null)
            {
                Debug.LogError(name + ": target is not an Agent.");
                return 0f;
            }

            // Get level based on leveltype - Put this in LevelType class?

            float level = otherAgent.GetLevel(levelType);


            return minMaxCurve.Eval0to100IgnoreMinMax(level);
        }
    }
}
