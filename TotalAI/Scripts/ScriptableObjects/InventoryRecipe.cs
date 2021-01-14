using System;
using System.Collections.Generic;
using UnityEngine;


namespace TotalAI
{
    [CreateAssetMenu(fileName = "InventoryRecipe", menuName = "Total AI/Inventory Recipe", order = 1)]
    public class InventoryRecipe : ScriptableObject
    {
        [Serializable]
        public class Recipe
        {
            public string state;
            public List<EntityType> inputEntityTypes;
            public List<int> inputAmounts;
            public List<EntityType> outputEntityTypes;
            public List<int> outputAmounts;
            public float timeInGameMinutes;
        }

        public List<Recipe> recipes;

        // Finds any recipes with the entityType as an input
        public List<Recipe> FindRecipesWithInput(EntityType entityType)
        {
            return recipes.FindAll(x => x.inputEntityTypes.Contains(entityType));
        }
    }
}