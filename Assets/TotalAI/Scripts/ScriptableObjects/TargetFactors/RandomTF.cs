using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "RandomTF", menuName = "Total AI/Target Factors/Random", order = 0)]
    public class RandomTF : TargetFactor
    {
        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            return minMaxCurve.EvalRandomIgnoreMinMax();
        }
    }
}
