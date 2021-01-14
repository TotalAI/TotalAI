using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "EntityTrigger", menuName = "Total AI/Entity Trigger", order = 1)]
    public class EntityTrigger : ScriptableObject
    {
        public enum TriggerType { MainLoop, UpdateLoop, LevelChange, OnTriggerEnter, OnTriggerExit, OnTriggerStay,
                                  OnCollisionEnter, OnCollisionExit, OnCollisionStay, OnParticleCollision };

        public TriggerType triggerType;
        [Tooltip("Flips the Entity and the target Entity causing the target to run the ICs and OCs.  Helpful if Entity " +
                 "is a WorldObject that impacts a target Agent since many ICs and OCs are created for Agents to run them.")]
        public bool forTarget;
        public LevelType levelType;
        public InputCondition.MatchType targetMatchType;
        public TypeGroup typeGroup;
        public TypeCategory typeCategory;
        public EntityType entityType;
        public float coolDown;

        public List<InputCondition> inputConditions;
        public List<OutputChange> outputChanges;

        private Dictionary<Entity, float> lastRan;

        private void OnEnable()
        {
            lastRan = new Dictionary<Entity, float>();
        }

        public void TryToRun(Entity entity, Entity target, float level = 0)
        {
            if (IsTriggered(entity, target, level))
            {
                Agent agent = entity as Agent;
                if (CheckInputConditions(agent, target))
                {
                    lastRan[entity] = Time.time;
                    bool succeeded = RunOutputChanges(agent, target);

                    if (!succeeded)
                    {
                        Debug.Log(agent.name + ": Failed OutputChange on  EntityTrigger " + name + " OCs - with target = " + target);
                    }
                }
                else
                {
                    Debug.Log(agent.name + ": InputConditions Failed for EntityTrigger " + name + " with target = " + target);
                }
            }
            else
            {
                Debug.Log(entity.name + ": IsTriggered failed for EntityTrigger " + name + " with target = " + target);
            }
        }

        private bool TypeMatch(EntityType targetEntityType)
        {
            switch (targetMatchType)
            {
                case InputCondition.MatchType.TypeGroup:
                    if (typeGroup == null || !typeGroup.InGroup(targetEntityType))
                        return false;
                    break;
                case InputCondition.MatchType.TypeCategory:
                    if (typeCategory == null || !typeCategory.IsCategoryOrDescendantOf(targetEntityType))
                        return false;
                    break;
                case InputCondition.MatchType.EntityType:
                    if (entityType == null || entityType != targetEntityType)
                        return false;
                    break;
            }
            return true;
        }

        private bool IsTriggered(Entity entity, Entity target, float level)
        {
            if (lastRan.TryGetValue(entity, out float time) && Time.time - time < coolDown)
                return false;

            switch (triggerType)
            {
                case TriggerType.MainLoop:
                case TriggerType.UpdateLoop:
                case TriggerType.LevelChange:
                    return true;
                case TriggerType.OnTriggerEnter:
                case TriggerType.OnTriggerExit:
                case TriggerType.OnTriggerStay:
                case TriggerType.OnCollisionEnter:
                case TriggerType.OnCollisionExit:
                case TriggerType.OnCollisionStay:
                case TriggerType.OnParticleCollision:
                    if ((!forTarget && TypeMatch(target.entityType)) || (forTarget && TypeMatch(entity.entityType)))
                        return true;
                    break;
            }
            return false;
        }

        private bool CheckInputConditions(Agent agent, Entity target)
        {
            foreach (InputCondition inputCondition in inputConditions)
            {
                if (!inputCondition.inputConditionType.Check(inputCondition, agent, null, target, false))
                    return false;
            }
            return true;
        }

        private bool RunOutputChanges(Agent agent, Entity target)
        {
            foreach (OutputChange outputChange in outputChanges)
            {
                Entity outputChangeTarget = target;
                if (outputChange.targetType == OutputChange.TargetType.ToSelf)
                    outputChangeTarget = agent;

                float amount = outputChange.outputChangeType.CalculateAmount(agent, outputChangeTarget, outputChange, null);
                
                if (!outputChange.CheckConditions(outputChangeTarget, agent, null, amount))
                {
                    Debug.Log(agent + ": EntityTrigger.RunOutputChanges - output conditions failed - " + this);
                    if (outputChange.stopType == OutputChange.StopType.OnOCCFailed)
                        return false;
                }

                bool succeeded = outputChange.outputChangeType.MakeChange(agent, outputChangeTarget, outputChange, null, amount, out bool forceStop);

                Debug.Log(agent + ": " + this + " EntityTrigger.RunOutputChanges - output condition " + outputChange.outputChangeType.name +
                          " succeeded = " + succeeded + " - target = " + target);

                if ((forceStop && !outputChange.blockMakeChangeForcedStop) ||
                    (outputChange.stopType == OutputChange.StopType.OnChangeFailed && !succeeded))
                    return false;

            }
            return true;
        }
    }
}
