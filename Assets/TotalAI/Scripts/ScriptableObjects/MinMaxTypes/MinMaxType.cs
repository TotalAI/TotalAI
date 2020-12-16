using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class MinMaxType : ScriptableObject
    {
        public abstract float GetMin(Agent agent);
        public abstract float GetMax(Agent agent);
        public abstract float GetDefaultValue(Agent agent);
    }
}

