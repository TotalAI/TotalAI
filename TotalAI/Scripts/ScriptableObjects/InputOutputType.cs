using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class InputOutputType : ScriptableObject
    {
        [Header("Value of One of this for Side-Effect Utility Calculation")]
        public float sideEffectValue = 1f;
        public List<TypeCategory> typeCategories;

        public string TypeCategoriesToString()
        {
            string result = "";
            foreach (TypeCategory typeCategory in typeCategories)
            {
                result += typeCategory.name + ", ";
            }
            return result.Length > 0 ? result.Remove(result.Length - 2, 2) : "None";
        }
    }
}