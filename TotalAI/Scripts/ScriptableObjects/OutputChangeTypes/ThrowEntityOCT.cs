using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ThrowEntityOCT", menuName = "Total AI/Output Change Types/Throw Entity", order = 0)]
    public class ThrowEntityOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Throw Entity</b>: Throws an Entity in Inventory towards the target.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.UnityObject },
                unityObjectType = typeof(MinMaxCurve),
                unityObjectLabel = "Random Degrees From Target",
                floatLabel = "Force to Apply",
                usesInventoryTypeGroupMatchIndex = true,
                usesFloatValue = true
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            Entity entityToThrow = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];

            if (entityToThrow == null)
            {
                Debug.LogError(agent.name + ": ThrowEntityOCT - No entity to throw found.");
                return false;
            }

            // Remove from inventory
            InventoryType.Item item = agent.inventoryType.Remove(agent, entityToThrow);
            if (item == null)
            {
                Debug.LogError(agent.name + ": ThrowEntityOCT - No entity to throw found in agent's inventory.");
                return false;
            }

            Vector3 targetPosition = target.transform.position;
            if (outputChange.unityObject != null)
            {
                // Move direction by random degrees
                MinMaxCurve minMaxCurve = outputChange.unityObject as MinMaxCurve;
                float degrees = minMaxCurve.EvalRandom();

                // Rotate targetPosition relative to agent.position by degrees
                Vector3 forceVector = Quaternion.Euler(0, 0, degrees) * ((Vector2)(targetPosition - agent.transform.position)).normalized;
                Debug.DrawRay(agent.transform.position, forceVector, Color.white, 5f);
                targetPosition = forceVector + agent.transform.position;
            }

            agent.inventoryType.Thrown(entityToThrow, agent, item.inventorySlot, targetPosition, outputChange.floatValue);
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 1f;
        }

        public override float CalculateSEUtility(Agent agent, Entity target, OutputChange outputChange,
                                                 Mapping mapping, DriveType mainDriveType, float amount)
        {
            return 0;
        }

        public override bool Match(Agent agent, OutputChange outputChange, MappingType outputChangeMappingType,
                                   InputCondition inputCondition, MappingType inputConditionMappingType)
        {
            return false;
        }
    }
}
