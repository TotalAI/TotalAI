
using System.Collections.Generic;

namespace TotalAI
{
    public class Action : Level
    {
        public enum InventorySkillModifierType { Add, Multiply, Override }

        public class InventorySkillModifier
        {
            public InventorySkillModifierType inventorySkillModifierType;
            public float skillLevel;

            public InventorySkillModifier(InventorySkillModifierType inventorySkillModifierType, float skillLevel)
            {
                this.inventorySkillModifierType = inventorySkillModifierType;
                this.skillLevel = skillLevel;
            }


            public float ModifyLevel(float level)
            {
                switch (inventorySkillModifierType)
                {
                    case InventorySkillModifierType.Add:
                        return level + skillLevel;
                    case InventorySkillModifierType.Multiply:
                        return level * skillLevel;
                    case InventorySkillModifierType.Override:
                        return skillLevel;
                }

                // This should never happen
                return level;
            }
        }
        
        private ActionType actionType;
        private float changeProbability;
        private float changeAmount;
        
        private Dictionary<EntityType, InventorySkillModifier> itemSkillModifiers;

        public Action(ActionType actionType, float level, float changeProbability, float changeAmount)
        {
            levelType = actionType;
            this.actionType = actionType;
            this.level = level;
            this.changeProbability = changeProbability;
            this.changeAmount = changeAmount;
            itemSkillModifiers = new Dictionary<EntityType, InventorySkillModifier>();
        }

        public override void SetActive(Agent agent, bool active)
        {
            // No change of status - just return
            if (active == !disabled)
                return;

            base.SetActive(agent, active);

            // Remove or Add any MappingTypes of this ActionType
            // TODO: What about the specific add and remove MTs on the Agent?  Maybe remove these and force user to make a role?
            List<MappingType> mappingTypes = actionType.FilteredMappingTypes(agent.agentType);
            if (active)
            {
                agent.availableMappingTypes.UnionWith(mappingTypes);
            }
            else
            {
                agent.availableMappingTypes.ExceptWith(mappingTypes);
            }
        }

        public void SetInventoryTypeSkill(EntityType entityType, InventorySkillModifierType inventorySkillModifierType, float skillLevel)
        {
            itemSkillModifiers.Add(entityType, new InventorySkillModifier(inventorySkillModifierType, skillLevel));
        }

        public float GetLevelWithItemActionSkillModifiers(Agent agent)
        {
            float level = GetLevel();
            level = agent.inventoryType.TotalItemActionSkillModifiersValue(agent, actionType, level);
            return level;
        }

        public override float GetLevel()
        {
            return level;
        }

        public override float ChangeLevel(float amount)
        {
            level = amount;
            return amount;
        }

        public float GetChangeProbability()
        {
            return changeProbability;
        }

        public float GetChangeAmount()
        {
            return changeAmount;
        }

        public void MaybeImproveActionSkill(Agent agent)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= changeProbability)
            {
                ChangeLevel(changeAmount);
            }
        }

    }
}
