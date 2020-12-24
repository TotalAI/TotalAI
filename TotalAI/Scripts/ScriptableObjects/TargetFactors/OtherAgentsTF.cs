using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "OtherAgentsTF", menuName = "Total AI/Target Factors/Other Agents", order = 0)]
    public class OtherAgentsTF : TargetFactor
    {
        // This target factor changes the likelihood of this entity being selected based on other agents actions
        private enum KnowledgeDictionary { Known, WithinRadius };
        private enum ChangeType { FixedUseMax, DistanceUseCurve }
        private enum StackType { Add, Mulitply }
        private enum IncOrDec { Increase, Decrease }

        [Header("All Known Agents or within a certain distance")]
        [SerializeField]
        private KnowledgeDictionary knowledgeDictionary = KnowledgeDictionary.Known;
        [SerializeField]
        private float radius = 0;
        [Header("Only consider agents that are in the agent's faction")]
        [SerializeField]
        private bool onlyAgentsFaction = false;
        [Header("Fixed utility or feed distance from target into curve")]
        [SerializeField]
        private ChangeType changeType = ChangeType.FixedUseMax;
        [Header("For multiple matching other agents - add or multiple utilities")]
        [SerializeField]
        private StackType stackType = StackType.Add;
        [Header("Encourage (increase) or discourage (decrease)")]
        [SerializeField]
        private IncOrDec incOrDec = IncOrDec.Increase;

        public override float Evaluate(Agent agent, Entity entity, Mapping mapping, bool forInventory)
        {
            float utility = 0;

            int numEntities = 0;
            int numMatchingEntities = 0;
            switch (knowledgeDictionary)
            {
                case KnowledgeDictionary.Known:
                    break;
                case KnowledgeDictionary.WithinRadius:
                    foreach (MemoryType.EntityInfo entityInfo in agent.memoryType.GetKnownEntities(agent, radius))
                    {
                        if (entityInfo.entity.entityType is AgentType)
                        {                            
                            ++numEntities;
                            Agent otherAgent = (Agent)entityInfo.entity;

                            // TODO: target is not great for some actions (i.e. collect and eat berries - eat berries has no target set)
                            if (otherAgent.decider.CurrentMapping != null && otherAgent.decider.CurrentMapping.target == entity &&
                                (!onlyAgentsFaction || otherAgent.faction == agent.faction))
                            {
                                ++numMatchingEntities;
                                if (changeType == ChangeType.FixedUseMax)
                                {
                                    if (stackType == StackType.Add)
                                        utility += minMaxCurve.max;
                                    else
                                        utility *= minMaxCurve.max;
                                }
                                else
                                {
                                    float distance = Vector3.Distance(otherAgent.transform.position, entity.transform.position);

                                    if (stackType == StackType.Add)
                                        utility += minMaxCurve.NormAndEvalIgnoreMinMax(distance, minMaxCurve.min, minMaxCurve.max);
                                    else
                                        utility *= minMaxCurve.NormAndEvalIgnoreMinMax(distance, minMaxCurve.min, minMaxCurve.max);
                                }
                            }                            
                        }
                    }
                    break;                
                default:
                    break;
            }

            if (incOrDec == IncOrDec.Decrease)
                utility = -utility;

            return utility;
        }
    }
}
