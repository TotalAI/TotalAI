using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "MinMaxCurve", menuName = "Total AI/Min Max Curve", order = 2)]
    public class MinMaxCurve : ScriptableObject
    {
        // Curve should produce values from 0-1 and accept levels of 0-1
        // this greatly simplifies the number of curves needed
        public AnimationCurve curve;
        public float min;
        public float max;

        private float expectedValue = float.PositiveInfinity;

        public float ExpectedValue()
        {
            if (!float.IsPositiveInfinity(expectedValue))
                return expectedValue;

            // Hasn't been calculated yet - calculate by sampling
            float calcValue = 0f;

            for (int i = 0; i < 1000; i++)
            {
                calcValue += EvalRandom();
            }
            expectedValue = calcValue / 1000f + min;
            return expectedValue;
        }

        public float Eval(float level)
        {
            if (min == max) return max;
            return min + curve.Evaluate(level) * (max - min);
        }

        public float EvalRandom()
        {
            if (min == max) return max;
            return min + curve.Evaluate(Random.Range(0f, 1f)) * (max - min);
        }

        public float EvalRandomIgnoreMinMax()
        {
            return curve.Evaluate(Random.Range(0f, 1f));
        }

        // Use for a return value between 0-1
        public float EvalIgnoreMinMax(float level)
        {
            return Mathf.Clamp(curve.Evaluate(level), 0, 1f);
        }

        public float NormAndEval(float level, float levelMin, float levelMax)
        {
            if (min == max) return max;

            if (level < levelMin)
                level = levelMin;
            else if (level > levelMax)
                level = levelMax;
            
            return Mathf.Clamp(min + curve.Evaluate((level - levelMin) / (levelMax - levelMin)) * (max - min), min, max);
        }

        // Use for a return value between 0-1
        public float NormAndEvalIgnoreMinMax(float level, float levelMin, float levelMax)
        {
            if (level < levelMin)
                level = levelMin;
            else if (level > levelMax)
                level = levelMax;

            return Mathf.Clamp(curve.Evaluate((level - levelMin) / (levelMax - levelMin)), 0, 1f);
        }

        // Simplified version since many levels go from 0-100
        public float Eval0to100(float level)
        {
            if (min == max) return max;
            return Mathf.Clamp(min + curve.Evaluate(level / 100f) * (max - min), min, max);
        }

        // Use for a return value between 0-1
        public float Eval0to100IgnoreMinMax(float level)
        {
            return Mathf.Clamp(curve.Evaluate(level / 100f), 0, 1f);
        }
    }
}
