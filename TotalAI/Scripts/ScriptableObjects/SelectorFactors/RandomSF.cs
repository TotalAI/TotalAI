using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RandomSF", menuName = "Total AI/Selector Factor/Random", order = 0)]
    public class RandomSF : SelectorFactor
    {
        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;

            return minMaxCurve.EvalRandom();
        }
    }
}