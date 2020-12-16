using UnityEngine;

namespace TotalAI
{
    public abstract class AnimationType : ScriptableObject
    {
        public abstract void SetupAgent(Agent agent, bool forEditor = false);
        public abstract Vector3 DeltaPosition(Agent agent);
        public abstract Vector3 RootPosition(Agent agent);

        public abstract void UpdateAnimations(Agent agent);

        public abstract float GetCurrentClipDuration(Agent agent, int layerIndex);
        public abstract string[] GetTriggerParamNames(Agent agent, bool forEditor);

        public abstract void SetFloat(Agent agent, string name, float value);
        public abstract void SetFloat(Agent agent, string name, float value, float dampTime, float deltaTime);
        public abstract void SetBool(Agent agent, string name, bool value);
        public abstract void SetTrigger(Agent agent, string name);

        public abstract void SetFloat(Agent agent, int id, float value);
        public abstract void SetFloat(Agent agent, int id, float value, float dampTime, float deltaTime);
        public abstract void SetBool(Agent agent, int id, bool value);
        public abstract void SetTrigger(Agent agent, int id);

        public abstract void PlayAnimationState(Agent agent, string stateName, int layerIndex = -1);
        public abstract void PlayAnimationState(Agent agent, int stateNameHash, int layerIndex = -1);

        public abstract bool InIdleState(Agent agent);
        public abstract void GoToIdleState(Agent agent);

        public abstract void SetAnimationSpeed(Agent agent, float speed);
        public abstract void ResetAnimationSpeed(Agent agent);

        public abstract void Disable(Agent agent);
        public abstract void Enable(Agent agent);

        public virtual bool ImplementsAnimationRigging()
        {
            return false;
        }
        public virtual void SetRigWeight(Agent agent, int rigLayerIndex, float weight, float speed = 3)
        {
        }
        public virtual void WarpRigConstraintTargetTo(Agent agent, int rigLayerIndex, Vector3 position, Quaternion rotation)
        {
        }
    }
}