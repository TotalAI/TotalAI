using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryMoveToContainerOCT", menuName = "Total AI/Output Change Types/Inventory Move To Container", order = 0)]
    public class InventoryMoveToContainerOCT : OutputChangeType
    {
        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Inventory Move To Container</b>: Moves an Entity in Inventory to another Entity in Inventory that can hold it.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.UnityObject },
                usesInventoryTypeGroupMatchIndex = true,
                usesFloatValue = true,
                floatLabel = "Amount To Move",
                unityObjectType = typeof(EntityType),
                unityObjectLabel = "EntityType to Move"
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            int intAmount = (int)actualAmount;
            if (intAmount > 0)
            {
                // TODO: Need to make sure its not taking item from the target container
                // TODO: Fix to not use unityObject
                List<InventoryType.Item> items = agent.inventoryType.Remove(agent, (EntityType)outputChange.unityObject, intAmount, true);
                if (items.Count != intAmount)
                {
                    Debug.LogError(agent.name + ": InventoryMoveToContainerOCT was unable to find Entity to Move (" + (EntityType)outputChange.unityObject +
                                   ").  Tried to take " + intAmount + " - found " + items.Count);
                    return false;
                }

                Entity entityContainer = mapping.inventoryTargets[outputChange.inventoryTypeGroupMatchIndex];
                if (entityContainer == null)
                {
                    Debug.LogError(agent.name + ": InventoryMoveToContainerOCT was unable to find Entity Container");
                    return false;
                }

                int numGiven = entityContainer.inventoryType.Add(entityContainer, items);
                if (numGiven != intAmount)
                {
                    // TODO: Handle partial gives
                    // Failed to Give items for some reason - give back inventory to agent
                    agent.inventoryType.Add(agent, items);

                    Debug.LogError(agent.name + ": InventoryMoveToContainerOCT was unable to give all to container.  Tried to give " +
                                   intAmount + " - gave " + numGiven);
                    return false;
                }
            }
            else
            {
                Debug.LogError(agent.name + ": InventoryMoveToContainerOCT has less than 1 amount to take.  Amount To Give must be positive.");
                return false;
            }
            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return outputChange.floatValue;
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
