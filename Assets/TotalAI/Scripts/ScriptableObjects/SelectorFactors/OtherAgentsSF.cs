using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherAgentsSF", menuName = "Total AI/Selector Factor/Other Agents", order = 0)]
    public class OtherAgentsSF : SelectorFactor
    {
        [Header("Ignore this param factor if number of other agents with same target is less than")]
        public float ignoreIfNumLessThan;

        [Header("Maxes out at the cap")]
        public float capNumberOfOtherAgents;

        [Header("Restrict to other agent's within RLevel range - min and max 0 to ignore this")]
        public float rLevelMin = 0f;
        public float rLevelMax = 0f;

        public override float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideFactor)
        {
            overrideFactor = false;

            // TODO: Use InteractionSpot class to make this check quick
            // Find other agents targeting same target
            Entity target = agent.decider.CurrentMapping.target;
            if (target == null) return float.NegativeInfinity;

            List<MemoryType.EntityInfo> entityInfos = agent.memoryType.GetKnownEntities(agent, MemoryType.EntityTypesEnum.AgentType);

            int numAgentsTargeting = 0;
            foreach (MemoryType.EntityInfo entityKnowledgeInfo in entityInfos)
            {
                if ((rLevelMin == 0f && rLevelMax == 0f) || (entityKnowledgeInfo.rLevel >= rLevelMin && entityKnowledgeInfo.rLevel <= rLevelMax))
                {
                    Agent otherAgent = (Agent)entityKnowledgeInfo.entity;

                    if (otherAgent.decider.CurrentMapping != null && otherAgent.decider.CurrentMapping.target == target)
                        numAgentsTargeting++;

                    if (numAgentsTargeting == capNumberOfOtherAgents)
                        break;
                }
            }

            if (numAgentsTargeting < ignoreIfNumLessThan) return float.NegativeInfinity;

            return minMaxCurve.Eval(numAgentsTargeting / capNumberOfOtherAgents);
        }
    }
}