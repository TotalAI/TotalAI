using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "TargetMovementSF", menuName = "Total AI/Selector Factor/Target Movement", order = 0)]
    public class TargetMovementSF : SelectorFactor
    {
        [Header("The min velocity of the agent for this SF to contribute")]
        public float agentVelocityMin = .1f;

        [Header("The min velocity of the target for this SF to contribute")]
        public float targetVelocityMin = .1f;

        [Header("The min cosine similarity value for this SF to contribute")]
        public float minCosineSimilarity = 0;

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;

            // TODO: Use InteractionSpot class to make this check quick
            // Find other agents targeting same target
            if (agent.decider.CurrentMapping.target == null || !(agent.decider.CurrentMapping.target is Agent))
                return float.NegativeInfinity;
            Agent target = (Agent)agent.decider.CurrentMapping.target;

            // TODO: navMeshAgent should be wrapped - so it is easy to swap in a different NavMesh System
            if (agent.movementType.VelocityMagnitude(agent) < agentVelocityMin)
                return float.NegativeInfinity;
            if (target.movementType.VelocityMagnitude(agent) < targetVelocityMin)
                return float.NegativeInfinity;

            // Cosine will give us a 1 to -1 value
            // 1 means the vectors point in the same direction so they are directly chasing each other
            // -1 means they are exactly moving away from each other
            float similarity = Mathf.Cos(Mathf.Deg2Rad * Vector3.Angle(agent.movementType.VelocityVector(agent), target.movementType.VelocityVector(agent)));

            if (similarity < minCosineSimilarity)
                return float.NegativeInfinity;

            return minMaxCurve.NormAndEvalIgnoreMinMax(similarity, -1f, 1f);
        }
    }
}