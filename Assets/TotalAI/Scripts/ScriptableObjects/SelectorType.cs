using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "SelectorType", menuName = "Total AI/Selector Type", order = 1)]
    public class SelectorType : ScriptableObject
    {
        public enum MinType { None, Min, CurrentValue, FixedValue }
        public MinType minType;
        public float minFixedValue;

        public enum StartType { Min, CurrentValue, FixedValue, Max }
        public StartType startType;
        public float startFixedValue;

        public enum MaxType { None, Max, CurrentValue, FixedValue }
        public MaxType maxType;
        public float maxFixedValue;

        public enum CombineType { NormalizedWeightedAverage, Sum, Multiply }
        public CombineType combineType;

        [Serializable]
        public class SelectorFactorInfo
        {
            public SelectorFactor selectorFactor;
            public float weight = 1f;
        }
        public List<SelectorFactorInfo> selectorFactorInfos;

        public int EvaluateEnumSelectorFactors(Selector selector, Agent agent, Mapping mapping, int numOptions)
        {
            // TODO: Handle restricting to subset of options
            // Seems hard - I think this can be accomplished by using ChoiceTypes?  Do the filtering in that.

            float result = EvaluateSelectorFactors(selector, agent, mapping);
            
            // The result is 0-1 - want to bin this into the number of options
            return BinResult(result, numOptions);
        }

        public float EvaluateFloatSelectorFactors(Selector selector, Agent agent, Mapping mapping, float minValue, float maxValue, float currentValue)
        {
            float startValue;
            switch (startType)
            {
                case StartType.Min:
                    startValue = minValue;
                    break;
                case StartType.CurrentValue:
                    startValue = currentValue;
                    break;
                case StartType.FixedValue:
                    startValue = startFixedValue;
                    break;
                default:
                    startValue = maxValue;
                    break;
            }
            float min;
            switch (minType)
            {
                case MinType.Min:
                    min = minValue;
                    break;
                case MinType.CurrentValue:
                    min = currentValue;
                    break;
                case MinType.FixedValue:
                    min = minFixedValue;
                    break;
                default:
                    min = float.NegativeInfinity;
                    break;
            }
            float max;
            switch (maxType)
            {
                case MaxType.Max:
                    max = maxValue;
                    break;
                case MaxType.CurrentValue:
                    max = currentValue;
                    break;
                case MaxType.FixedValue:
                    max = maxFixedValue;
                    break;
                default:
                    max = float.PositiveInfinity;
                    break;
            }

            float result = EvaluateSelectorFactors(selector, agent, mapping);

            if (combineType == CombineType.NormalizedWeightedAverage)
            {
                //Debug.Log("RESULT = " + result + " - " + (result / count) * (max - min) + min);
                return result * (max - min) + min;
            }

            result += startValue;
            return Mathf.Clamp(result, min, max);
        }

        private int BinResult(float result, int numOptions)
        {
            // Generate break points from 1 to x - divide by numOptions - 1/x, 2/x ... 1
            float[] breakPoints = Enumerable.Range(1, numOptions).Select(x => x / (float)numOptions).ToArray();
            for (int i = 0; i < numOptions; i++)
            {
                if (result <= breakPoints[i])
                    return i;
            }
            Debug.LogError("This should never happen - Unable to find a bin in BinResult: " + result + " : " + breakPoints[numOptions - 1]);
            return numOptions - 1;
        }

        // Returns -1f if there were no Factors
        private float EvaluateSelectorFactors(Selector selector, Agent agent, Mapping mapping)
        {
            float finalResult = 0f;
            float weightedCount = 0f;
            foreach (SelectorFactorInfo selectorFactorInfo in selectorFactorInfos)
            {
                // Do weighted average ones and then multiplier ones?
                float result = selectorFactorInfo.selectorFactor.Evaluate(selector, agent, mapping, out bool overrideFactor);

                if (overrideFactor)
                {
                    Debug.Log(agent.name + ": ParamFactor VETO - " + selectorFactorInfo.selectorFactor + " result = " + result);
                    return result;
                }

                // An SelectorFactor can return -inf if it should not contribute
                if (float.IsNegativeInfinity(result)) continue;

                switch (combineType)
                {
                    case CombineType.NormalizedWeightedAverage:
                        finalResult += result * selectorFactorInfo.weight;
                        weightedCount += selectorFactorInfo.weight;
                        break;
                    case CombineType.Sum:
                        finalResult += result;
                        break;
                    case CombineType.Multiply:
                        finalResult *= result;
                        break;
                }
            }

            if (combineType == CombineType.NormalizedWeightedAverage)
            {
                if (weightedCount == 0)
                {
                    // TODO: Want to have a default value if there are no SFs that apply
                    Debug.LogError(agent.name + ": Missing SelectorFactors (weight == 0) for MT: " + mapping.mappingType.name);
                    return 0f;
                }
                return Mathf.Clamp(finalResult / weightedCount, 0f, 1f);
            }

            return finalResult;
        }
    }
}