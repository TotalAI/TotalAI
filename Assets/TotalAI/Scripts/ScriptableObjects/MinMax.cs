using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MinMax", menuName = "Total AI/Min Max", order = 2)]
    public class MinMax : ScriptableObject
    {
        [Header("Set either Min Max Type OR Min, Max, and Default Value")]
        public MinMaxType minMaxType;
        public float min;
        public float max;
        public float defaultValue;

        public float GetMin(Agent agent)
        {
            if (minMaxType != null)
                return minMaxType.GetMin(agent);
            return min;
        }

        public float GetMax(Agent agent)
        {
            if (minMaxType != null)
                return minMaxType.GetMax(agent);
            return max;
        }

        public float DefaultValue(Agent agent)
        {
            if (minMaxType != null)
                return minMaxType.GetDefaultValue(agent);
            return defaultValue;
        }
    }
}
