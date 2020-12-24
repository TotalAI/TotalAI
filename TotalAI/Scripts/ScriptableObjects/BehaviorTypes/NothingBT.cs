using System;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NothingBT", menuName = "Total AI/Behavior Types/Nothing", order = 0)]
    public class NothingBT : BehaviorType
    {
        private new void OnEnable()
        {
            editorHelp = new EditorHelp()
            {
                valueTypes = new Type[] {},
                valueDescriptions = new string[] {}
            };
        }

        public override void SetContext(Agent agent, Behavior.Context behaviorContext)
        {
        }

        public override void StartBehavior(Agent agent)
        {
        }

        public override void UpdateBehavior(Agent agent)
        {
        }

        public override bool IsFinished(Agent agent)
        {
            return true;
        }

        public override void InterruptBehavior(Agent agent)
        {
        }

        // Return the estimated time to complete this mapping
        public override float EstimatedTimeToComplete(Agent agent, Mapping mapping)
        {
            return 0.1f;
        }
    }
}
