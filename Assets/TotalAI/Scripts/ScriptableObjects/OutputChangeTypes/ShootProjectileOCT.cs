using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    [CreateAssetMenu(fileName = "ShootProjectileOCT", menuName = "Total AI/Output Change Types/Shoot Projectile", order = 0)]
    public class ShootProjectileOCT : OutputChangeType
    {
        public TypeCategory projectileCategory;
        public InventorySlot shooterSlot;
        public InventorySlot projectileSlot;

        public new void OnEnable()
        {
            typeInfo = new TypeInfo()
            {
                description = "<b>Shoot Projectile</b>: Shoots out a projectile.  Speed and damage is calculated by the projectile.",
                possibleTargetTypes = new OutputChange.TargetType[] { OutputChange.TargetType.ToEntityTarget },
                possibleValueTypes = new OutputChange.ValueType[] { OutputChange.ValueType.None },
            };
        }

        public override bool MakeChange(Agent agent, Entity target, OutputChange outputChange, Mapping mapping, float actualAmount, out bool forceStop)
        {
            forceStop = false;

            // Make sure Agent is holding something that can shoot projectiles
            // These checks should be done by the ICs so maybe ckip them here?
            // TODO: Be nice to have the inventory entity target for this
            WorldObject shooter = agent.inventoryType.GetFirstEntityInSlot(agent, shooterSlot) as WorldObject;
            if (shooter == null)
            {
                Debug.LogError(agent.name + ": ShootProjectileOCT - No ranged weapon equipped");
                return false;
            }

            // Remove projectile from shooter's inventory
            InventoryType.Item projectileItem = shooter.inventoryType.Remove(shooter, projectileSlot);
            if (projectileItem == null)
            {
                Debug.LogError(agent.name + ": ShootProjectileOCT - No projectileWorldObject in " + shooter.name);
                return false;
            }

            projectileItem.entity.transform.parent = null;

            // Shoot it
            Projectile projectile = projectileItem.entity.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError(agent.name + ": ShootProjectileOCT - No projectile Component on the projectileWorldObject " + projectileItem.entity.name);
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
