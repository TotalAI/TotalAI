using System;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class BehaviorType : ScriptableObject
    {
        public List<AttributeType> requiredAttributeTypes;

        [Header("Default Selectors for Mapping Types")]
        public List<Selector> defaultSelectors;

        [Header("Coroutine Wait Times")]
        public float afterStartWaitTime = 0.3f;
        public float afterUpdateWaitTime = 0.3f;
        public float beforeFinishWaitTime = 0.0f;

        public abstract void SetContext(Agent agent, Behavior.Context behaviorContext);
        public abstract void StartBehavior(Agent agent);
        public abstract void UpdateBehavior(Agent agent);
        public abstract bool IsFinished(Agent agent);
        public abstract void InterruptBehavior(Agent agent);
        public abstract float EstimatedTimeToComplete(Agent agent, Mapping mapping);

        // These fields are used to aid in the creating of MappingTypes
        public class EditorHelp
        {
            public Type[] valueTypes;
            public string[] valueDescriptions;
            public Type[] requiredAttributeTypes;
            public string[] requiredDescriptions;
        }

        public EditorHelp editorHelp;

        public void OnEnable()
        {
            editorHelp = new EditorHelp();
        }
    }
}