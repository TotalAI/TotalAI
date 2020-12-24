using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "NoneAT", menuName = "Total AI/Animation Types/None", order = 0)]
    public class NoneAT : AnimationType
    {
        public override void SetupAgent(Agent agent, bool forEditor = false)
        {
        }
        public override Vector3 DeltaPosition(Agent agent)
        {
            return Vector3.positiveInfinity;
        }
        public override Vector3 RootPosition(Agent agent)
        {
            return Vector3.positiveInfinity;
        }
        public override void UpdateAnimations(Agent agent)
        {
        }
        public override float GetCurrentClipDuration(Agent agent, int layerIndex)
        {
            return 0f;
        }
        public override string[] GetTriggerParamNames(Agent agent, bool forEditor)
        {
            return null;
        }
        public override void SetFloat(Agent agent, string name, float value)
        {
        }
        public override void SetFloat(Agent agent, string name, float value, float dampTime, float deltaTime)
        {
        }
        public override void SetBool(Agent agent, string name, bool value)
        {
        }
        public override void SetTrigger(Agent agent, string name)
        {
        }
        public override void SetFloat(Agent agent, int id, float value)
        {
        }
        public override void SetFloat(Agent agent, int id, float value, float dampTime, float deltaTime)
        {
        }
        public override void SetBool(Agent agent, int id, bool value)
        {
        }
        public override void SetTrigger(Agent agent, int id)
        {
        }
        public override void PlayAnimationState(Agent agent, string stateName, int layerIndex = -1)
        {
        }
        public override void PlayAnimationState(Agent agent, int stateNameHash, int layerIndex = -1)
        {
        }
        public override bool InIdleState(Agent agent)
        {
            return true;
        }
        public override void GoToIdleState(Agent agent)
        {
        }
        public override void SetAnimationSpeed(Agent agent, float speed)
        {
        }
        public override void ResetAnimationSpeed(Agent agent)
        {
        }
        public override void Disable(Agent agent)
        {
        }
        public override void Enable(Agent agent)
        {
        }
    }
}