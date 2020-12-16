using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ThrowProjectileOCT", menuName = "Total AI/Output Change Types/Throw Projectile", order = 0)]
    public class ThrowProjectileOCT : OutputChangeType
    {

        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Throw Projectile</b>: Throws inventory target projectile. None value type will use created Entity from previous OC.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None, OutputChange.ValueType.FloatValue },
                floatLabel = "Inventory Target Index From ICs"
                //usesInventoryTypeGroupMatchIndex = true,
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            Entity projectileEntity;
            if (outputChange.valueType == OutputChange.ValueType.None)
            {
                OutputChange previousOutputChange = outputChange.Previous(mapping.mappingType);
                projectileEntity = agent.inventoryType.GetFirstEntityofEntityType(agent, previousOutputChange.entityType);
            }
            else
            {
                projectileEntity = mapping.inventoryTargets[(int)outputChange.floatValue];
            }

            // Remove projectile from shooter's inventory
            InventoryType.Item projectileItem = agent.inventoryType.Remove(agent, projectileEntity);
            if (projectileItem == null)
            {
                Debug.LogError(agent.name + ": ThrowProjectileOCT - No projectile WorldObject - " + projectileEntity.name);
                return false;
            }

            // TODO: This is strange here - Need a Throw method in InventoryType to handle this?
            projectileItem.entity.transform.parent = null;
            projectileItem.entity.inEntityInventory = null;

            // Throw it
            Projectile projectile = projectileItem.entity.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError(agent.name + ": ThrowProjectileOCT - No Projectile Component on the projectile WorldObject " + projectileEntity.name);
                return false;
            }
            projectile.target = target.gameObject;

            return true;
        }

        public override float CalculateAmount(Agent agent, Entity target, OutputChange outputChange, Mapping mapping)
        {
            return 0f;
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
